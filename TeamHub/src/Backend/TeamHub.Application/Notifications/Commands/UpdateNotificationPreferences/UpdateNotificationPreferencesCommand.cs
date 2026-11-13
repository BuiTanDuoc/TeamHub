using Mediator;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications.Commands.UpdateNotificationPreferences;

/// <summary>Lưu cài đặt cho các loại thông báo được gửi lên; loại không có trong danh sách giữ nguyên.</summary>
public sealed record UpdateNotificationPreferencesCommand(IReadOnlyList<NotificationPreferenceItem> Items) : ICommand<Unit>;

public sealed record NotificationPreferenceItem(NotificationType Type, bool InAppEnabled, bool PushEnabled);
