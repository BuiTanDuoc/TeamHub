using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Bugs.Commands.ChangeBugStatus;

public sealed class ChangeBugStatusHandler(
    IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, INotificationDispatcher notifications)
    : ICommandHandler<ChangeBugStatusCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangeBugStatusCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");
        var role = await db.ResolveActorRoleAsync(currentUser, command.TeamId, cancellationToken);

        var bug = await db.Bugs
            .SingleOrDefaultAsync(b => b.Id == command.BugId && b.TeamId == command.TeamId
                && b.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Bug", command.BugId);

        // Workflow, vai trò và lý do bắt buộc nằm ở Domain (BugWorkflow); đồng thời ghi BugStatusHistory
        bug.ChangeStatus(command.Status, userId, role, timeProvider.GetUtcNow(), command.Note);

        if (!string.IsNullOrWhiteSpace(command.FixedInVersion))
            bug.SetFixedInVersion(command.FixedInVersion);

        // Báo cho người xử lý và người báo bug (trừ người vừa đổi trạng thái)
        await notifications.StageBugEventAsync(bug, userId, NotificationType.BugStatusChanged,
            bug.Status.ToString(), [bug.AssigneeId, bug.ReporterId], cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return Unit.Value;
    }
}
