using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using TeamHub.Application.Notifications;

namespace TeamHub.Api.Push;

/// <summary>
/// Gửi push qua Firebase Cloud Messaging (mỗi lần tối đa 500 tin) và trả về các token không còn hợp lệ.
/// Gửi tin chỉ có "data" (kèm title/body) để service worker của web tự hiển thị và tự quyết định đích khi người dùng bấm.
/// </summary>
public sealed class FcmPushSender(IOptions<FcmPushOptions> options, ILogger<FcmPushSender> logger) : IPushSender
{
    private const string AppName = "teamhub-fcm";

    // Giới hạn của FCM cho mỗi lần SendEachAsync
    private const int MaxMessagesPerBatch = 500;

    // Khởi tạo lười: app chạy được khi chưa cấu hình Firebase (Enabled = false), lỗi credential chỉ lộ ra khi gửi
    private readonly Lazy<FirebaseMessaging> _messaging = new(() => CreateMessaging(options.Value));

    public async Task<IReadOnlyList<string>> SendAsync(IReadOnlyList<PushMessage> messages, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var messaging = _messaging.Value;
        var invalidTokens = new List<string>();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));

        foreach (var chunk in messages.Chunk(MaxMessagesPerBatch))
        {
            var batch = await messaging.SendEachAsync(chunk.Select(m => ToFcmMessage(m, settings)).ToList(), timeout.Token);
            if (batch.FailureCount == 0)
                continue;

            // Kết quả trả về cùng thứ tự với tin đã gửi
            for (var i = 0; i < batch.Responses.Count; i++)
            {
                var response = batch.Responses[i];
                if (response.IsSuccess)
                    continue;

                // Chỉ UNREGISTERED mới chắc chắn là token chết; các lỗi khác (quota, cấu hình sai project...) giữ token lại
                if (response.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    invalidTokens.Add(chunk[i].To);
                else
                    logger.LogWarning(response.Exception, "FCM từ chối một tin push (mã lỗi {ErrorCode})",
                        response.Exception?.MessagingErrorCode);
            }
        }

        return invalidTokens;
    }

    private static Message ToFcmMessage(PushMessage message, FcmPushOptions settings)
    {
        // title/body nằm trong data để service worker dựng thông báo; khóa data chỉ gồm chuỗi (yêu cầu của FCM)
        var data = new Dictionary<string, string>(message.Data)
        {
            ["title"] = message.Title,
            ["body"] = message.Body
        };
#pragma warning disable CS0618

        return new Message
        {
            Token = message.To,
            Data = data,
            Webpush = new WebpushConfig
            {
                Headers = new Dictionary<string, string>
                {
                    ["TTL"] = settings.TimeToLiveSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["Urgency"] = "high"
                }
            },
            Android = new AndroidConfig
            {
                Priority = FirebaseAdmin.Messaging.Priority.High,
                TimeToLive = TimeSpan.FromSeconds(settings.TimeToLiveSeconds)
            }
        };
#pragma warning restore CS0618
    }

    private static FirebaseMessaging CreateMessaging(FcmPushOptions settings)
    {
        var app = FirebaseApp.GetInstance(AppName) ?? FirebaseApp.Create(
            new AppOptions { Credential = LoadCredential(settings) }, AppName);
        return FirebaseMessaging.GetMessaging(app);
    }

    private static GoogleCredential? LoadCredential(FcmPushOptions settings)
    {
        var json = settings.CredentialJson;
        if (string.IsNullOrWhiteSpace(json) && !string.IsNullOrWhiteSpace(settings.CredentialFile))
            json = File.ReadAllText(settings.CredentialFile);

        // Null → Firebase Admin SDK dùng Application Default Credentials (GOOGLE_APPLICATION_CREDENTIALS, workload identity)
        return string.IsNullOrWhiteSpace(json)
            ? null
            : CredentialFactory.FromJson<ServiceAccountCredential>(json).ToGoogleCredential();
    }
}
