namespace TeamHub.Api.Jobs;

/// <summary>Cấu hình job dọn thông báo cũ, mục "Notifications:Cleanup" trong appsettings.json.</summary>
public sealed class NotificationCleanupOptions
{
    public const string SectionName = "Notifications:Cleanup";
    public const string JobId = "notification-cleanup";

    /// <summary>Lịch chạy dạng cron, tính theo UTC (mặc định 20:00 UTC = 03:00 giờ Việt Nam).</summary>
    public string Cron { get; set; } = "0 20 * * *";

    /// <summary>Xóa thông báo đã đọc cũ hơn số ngày này.</summary>
    public int ReadRetentionDays { get; set; } = 30;

    /// <summary>Xóa mọi thông báo (kể cả chưa đọc) cũ hơn số ngày này.</summary>
    public int MaxRetentionDays { get; set; } = 180;

    public int BatchSize { get; set; } = 1000;
}
