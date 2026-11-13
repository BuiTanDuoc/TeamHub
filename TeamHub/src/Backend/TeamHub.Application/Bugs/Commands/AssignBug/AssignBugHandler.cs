using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Bugs.Commands.AssignBug;

public sealed class AssignBugHandler(
    IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, INotificationDispatcher notifications)
    : ICommandHandler<AssignBugCommand, Unit>
{
    public async ValueTask<Unit> Handle(AssignBugCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");
        var role = await db.ResolveActorRoleAsync(currentUser, command.TeamId, cancellationToken);

        var bug = await db.Bugs
            .SingleOrDefaultAsync(b => b.Id == command.BugId && b.TeamId == command.TeamId
                && b.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Bug", command.BugId);

        var isProjectMember = await db.ProjectMembers
            .AnyAsync(m => m.ProjectId == command.ProjectId && m.UserId == command.AssigneeId, cancellationToken);
        if (!isProjectMember)
            throw new DomainException("Người được giao phải là thành viên của dự án.");

        bug.Assign(command.AssigneeId, userId, role, timeProvider.GetUtcNow());

        await notifications.StageBugEventAsync(bug, userId, NotificationType.BugAssigned, null,
            [command.AssigneeId], cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return Unit.Value;
    }
}
