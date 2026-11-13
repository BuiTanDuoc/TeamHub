using Mediator;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Notifications.Queries.GetMyNotifications;

/// <summary>Thông báo của tôi, mới nhất trước; UnreadOnly để chỉ lấy thông báo chưa đọc.</summary>
public sealed record GetMyNotificationsQuery(bool UnreadOnly, int Page, int PageSize)
    : IQuery<PagedResult<NotificationDto>>, IPagedQuery;
