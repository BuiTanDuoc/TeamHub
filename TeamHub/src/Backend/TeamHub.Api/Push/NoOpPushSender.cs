using TeamHub.Application.Notifications;

namespace TeamHub.Api.Push;

/// <summary>Dùng khi Push:Expo:Enabled = false: không gửi gì.</summary>
public sealed class NoOpPushSender : IPushSender
{
    public Task<IReadOnlyList<string>> SendAsync(IReadOnlyList<PushMessage> messages, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>([]);
}
