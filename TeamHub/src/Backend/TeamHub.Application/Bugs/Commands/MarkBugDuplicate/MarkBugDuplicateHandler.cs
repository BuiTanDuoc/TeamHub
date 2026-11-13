using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Bugs.Commands.MarkBugDuplicate;

public sealed class MarkBugDuplicateHandler(
    IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, INotificationDispatcher notifications)
    : ICommandHandler<MarkBugDuplicateCommand, Unit>
{
    public async ValueTask<Unit> Handle(MarkBugDuplicateCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");
        var role = await db.ResolveActorRoleAsync(currentUser, command.TeamId, cancellationToken);

        var bug = await db.Bugs
            .SingleOrDefaultAsync(b => b.Id == command.BugId && b.TeamId == command.TeamId
                && b.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Bug", command.BugId);

        var originalExists = await db.Bugs.AnyAsync(b => b.Id == command.OriginalBugId
            && b.TeamId == command.TeamId && b.ProjectId == command.ProjectId, cancellationToken);
        if (!originalExists)
            throw new NotFoundException("Bug gốc", command.OriginalBugId);

        bug.MarkDuplicate(command.OriginalBugId, userId, role, timeProvider.GetUtcNow(), command.Note);

        await notifications.StageBugEventAsync(bug, userId, NotificationType.BugStatusChanged,
            bug.Status.ToString(), [bug.AssigneeId, bug.ReporterId], cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return Unit.Value;
    }
}
