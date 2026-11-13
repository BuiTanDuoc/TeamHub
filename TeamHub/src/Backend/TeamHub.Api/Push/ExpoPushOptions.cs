namespace TeamHub.Api.Push;

/// <summary>Cấu hình gửi push qua Expo Push Service (Expo chuyển tiếp tới FCM/APNs), mục "Push:Expo".</summary>
public sealed class ExpoPushOptions
{
    public const string SectionName = "Push:Expo";

    /// <summary>Tắt để không gửi push (môi trường test/dev không cần).</summary>
    public bool Enabled { get; set; } = true;

    public string Endpoint { get; set; } = "https://exp.host/--/api/v2/push/send";

    /// <summary>Access token của Expo, chỉ cần khi dự án bật "enhanced push security". Đặt bằng user-secrets, không commit.</summary>
    public string? AccessToken { get; set; }

    /// <summary>Thời gian chờ tối đa mỗi lần gọi (giây). Ngắn để không làm chậm request nghiệp vụ gốc.</summary>
    public int TimeoutSeconds { get; set; } = 5;
}
