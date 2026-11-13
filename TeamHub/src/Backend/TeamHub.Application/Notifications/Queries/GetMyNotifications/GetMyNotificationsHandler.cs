using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<GetMyNotificationsQuery, KeysetPagedResult<NotificationDto>>
{
    public async ValueTask<KeysetPagedResult<NotificationDto>> Handle(
        GetMyNotificationsQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var notifications = db.Notifications.AsNoTracking().Where(n => n.RecipientId == userId);
        if (query.UnreadOnly)
            notifications = notifications.Where(n => !n.IsRead);

        // Validator đã bảo đảm cursor (nếu có) giải mã được
        if (query.Cursor is not null && NotificationCursor.TryDecode(query.Cursor, out var createdAt, out var lastId))
        {
            // Cùng thứ tự với ORDER BY bên dưới (CreatedAt giảm dần, rồi Id giảm dần)
            notifications = notifications.Where(n =>
                n.CreatedAt < createdAt || (n.CreatedAt == createdAt && n.Id.CompareTo(lastId) < 0));
        }

        // Lấy dư một phần tử để biết còn trang sau hay không
        var page = await notifications
            .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
            .Take(query.PageSize + 1)
            .ToDtos()
            .ToListAsync(cancellationToken);

        var hasMore = page.Count > query.PageSize;
        var items = hasMore ? page.Take(query.PageSize).ToList() : page;

        return new KeysetPagedResult<NotificationDto>(
            items, hasMore ? NotificationCursor.Encode(items[^1].CreatedAt, items[^1].Id) : null);
    }
}
