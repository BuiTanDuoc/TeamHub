using Mediator;

namespace TeamHub.Application.Notifications.Commands.MarkAllNotificationsRead;

/// <summary>Đánh dấu đã đọc toàn bộ thông báo của tôi. Trả về số thông báo được cập nhật.</summary>
public sealed record MarkAllNotificationsReadCommand : ICommand<int>;
