using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Notifications.Queries.GetUnreadNotificationCount;

public sealed class GetUnreadNotificationCountHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<GetUnreadNotificationCountQuery, UnreadCountDto>
{
    public async ValueTask<UnreadCountDto> Handle(GetUnreadNotificationCountQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var count = await db.Notifications.AsNoTracking()
            .CountAsync(n => n.RecipientId == userId && !n.IsRead, cancellationToken);

        return new UnreadCountDto(count);
    }
}
