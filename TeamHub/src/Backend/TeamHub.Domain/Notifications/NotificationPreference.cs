using TeamHub.Domain.Common;

namespace TeamHub.Domain.Notifications;

/// <summary>
/// Cài đặt nhận thông báo của một người cho từng loại. Không có bản ghi nghĩa là bật tất cả kênh.
/// InApp = lưu thông báo trong app và đẩy realtime; Push = đẩy tới điện thoại.
/// </summary>
public sealed class NotificationPreference : BaseEntity
{
    private NotificationPreference() { } // EF Core

    public NotificationPreference(Guid userId, NotificationType type, bool inAppEnabled, bool pushEnabled)
    {
        UserId = userId;
        Type = type;
        InAppEnabled = inAppEnabled;
        PushEnabled = pushEnabled;
    }

    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public bool InAppEnabled { get; private set; }
    public bool PushEnabled { get; private set; }

    public void Update(bool inAppEnabled, bool pushEnabled)
    {
        InAppEnabled = inAppEnabled;
        PushEnabled = pushEnabled;
    }
}
