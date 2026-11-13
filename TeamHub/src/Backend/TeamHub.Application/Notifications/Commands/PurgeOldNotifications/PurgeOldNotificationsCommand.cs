using Mediator;

namespace TeamHub.Application.Notifications.Commands.PurgeOldNotifications;

/// <summary>
/// Dọn thông báo cũ: xóa thông báo ĐÃ ĐỌC tạo trước ReadBefore, và mọi thông báo (kể cả chưa đọc) tạo trước
/// AnyBefore. Chạy từ job nền, không gắn với user nào. Trả về tổng số thông báo đã xóa.
/// </summary>
public sealed record PurgeOldNotificationsCommand(DateTimeOffset ReadBefore, DateTimeOffset AnyBefore, int BatchSize)
    : ICommand<int>;
