using Microsoft.Extensions.Options;
using TeamHub.Application.Notifications;

namespace TeamHub.Api.Push;

/// <summary>
/// Chia tin theo định dạng token: Expo token → Expo Push Service, còn lại → FCM. Mỗi kênh tắt/bật riêng bằng
/// <c>Push:Expo:Enabled</c> và <c>Push:Fcm:Enabled</c>; một kênh lỗi không làm mất tin của kênh kia.
/// </summary>
public sealed class PushRouter(
    ExpoPushSender expo, FcmPushSender fcm,
    IOptions<ExpoPushOptions> expoOptions, IOptions<FcmPushOptions> fcmOptions,
    ILogger<PushRouter> logger) : IPushSender
{
    public async Task<IReadOnlyList<string>> SendAsync(IReadOnlyList<PushMessage> messages, CancellationToken cancellationToken)
    {
        var invalidTokens = new List<string>();

        if (expoOptions.Value.Enabled)
            await SendSafelyAsync("Expo", expo, messages.Where(m => PushTokenFormat.IsExpo(m.To)).ToList(), invalidTokens, cancellationToken);

        if (fcmOptions.Value.Enabled)
            await SendSafelyAsync("FCM", fcm, messages.Where(m => !PushTokenFormat.IsExpo(m.To)).ToList(), invalidTokens, cancellationToken);

        return invalidTokens;
    }

    private async Task SendSafelyAsync(string channel, IPushSender sender, List<PushMessage> messages,
        List<string> invalidTokens, CancellationToken cancellationToken)
    {
        if (messages.Count == 0)
            return;

        try
        {
            invalidTokens.AddRange(await sender.SendAsync(messages, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được {Count} tin push qua {Channel}", messages.Count, channel);
        }
    }
}
