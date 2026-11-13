using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<GetMyNotificationsQuery, PagedResult<NotificationDto>>
{
    public async ValueTask<PagedResult<NotificationDto>> Handle(GetMyNotificationsQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var notifications = db.Notifications.AsNoTracking().Where(n => n.RecipientId == userId);
        if (query.UnreadOnly)
            notifications = notifications.Where(n => !n.IsRead);

        return await notifications
            .OrderByDescending(n => n.CreatedAt).ThenBy(n => n.Id)
            .ToDtos()
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
