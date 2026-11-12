using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Common.Behaviors;

/// <summary>Nơi duy nhất kiểm tra quyền theo team/SystemAdmin; không check quyền rải rác trong handler.</summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(ICurrentUser currentUser, IAppDbContext db)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        if (message is IRequireSystemAdmin && !currentUser.IsSystemAdmin)
            throw new ForbiddenException("Chỉ SystemAdmin được thực hiện thao tác này.");

        if (message is IRequireTeamRole required && !currentUser.IsSystemAdmin)
        {
            var userId = currentUser.UserId
                ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

            // TeamMembers không có query filter nên phải lọc rõ theo TeamId và UserId
            var role = await db.TeamMembers
                .AsNoTracking()
                .Where(m => m.TeamId == required.TeamId && m.UserId == userId)
                .Select(m => (TeamRole?)m.Role)
                .SingleOrDefaultAsync(cancellationToken);

            if (role is null || role < required.MinimumRole)
                throw new ForbiddenException("Bạn không có quyền thực hiện thao tác này trong team.");
        }

        return await next(message, cancellationToken);
    }
}
