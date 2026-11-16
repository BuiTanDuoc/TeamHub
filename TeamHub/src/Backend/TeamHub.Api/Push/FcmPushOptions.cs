namespace TeamHub.Api.Push;

/// <summary>Cấu hình gửi push qua Firebase Cloud Messaging (HTTP v1, dùng Firebase Admin SDK), mục "Push:Fcm".</summary>
public sealed class FcmPushOptions
{
    public const string SectionName = "Push:Fcm";

    /// <summary>Mặc định tắt: chỉ bật sau khi đã có service account của dự án Firebase.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Đường dẫn file JSON service account (Firebase Console → Project settings → Service accounts → Generate new private key).
    /// Không commit file này. Nếu bỏ trống cả hai mục Credential*, dùng Application Default Credentials (GOOGLE_APPLICATION_CREDENTIALS).
    /// </summary>
    public string? CredentialFile { get; set; }

    /// <summary>Nội dung JSON service account, tiện cho biến môi trường/secret manager (Push__Fcm__CredentialJson). Ưu tiên hơn CredentialFile.</summary>
    public string? CredentialJson { get; set; }

    /// <summary>Thời gian chờ tối đa cho cả lượt gửi (giây). Ngắn để không làm chậm request nghiệp vụ gốc.</summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>Thời gian FCM giữ tin khi thiết bị offline (giây); quá hạn thì bỏ vì thông báo cũ không còn giá trị.</summary>
    public int TimeToLiveSeconds { get; set; } = 86400;
}
