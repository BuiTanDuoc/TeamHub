using Mediator;

namespace TeamHub.Application.Notifications.Queries.GetNotificationPreferences;

/// <summary>Cài đặt nhận thông báo của tôi cho mọi loại thông báo (loại chưa cài đặt mặc định bật tất cả kênh).</summary>
public sealed record GetNotificationPreferencesQuery : IQuery<IReadOnlyList<NotificationPreferenceDto>>;
