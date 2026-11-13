using Mediator;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Notifications.Queries.GetMyNotifications;

/// <summary>
/// Thông báo của tôi, mới nhất trước; UnreadOnly để chỉ lấy chưa đọc. Keyset pagination: truyền NextCursor
/// của trang trước vào Cursor để lấy trang kế tiếp (không bị lặp/sót khi có thông báo mới đến giữa chừng).
/// </summary>
public sealed record GetMyNotificationsQuery(bool UnreadOnly, string? Cursor, int PageSize)
    : IQuery<KeysetPagedResult<NotificationDto>>;
