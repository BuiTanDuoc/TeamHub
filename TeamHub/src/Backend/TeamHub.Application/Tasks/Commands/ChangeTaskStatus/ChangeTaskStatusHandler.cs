using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusHandler(
    IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, INotificationDispatcher notifications)
    : ICommandHandler<ChangeTaskStatusCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangeTaskStatusCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var item = await db.WorkItems
            .SingleOrDefaultAsync(w => w.Id == command.TaskId && w.TeamId == command.TeamId
                && w.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Task", command.TaskId);

        var from = item.Status;
        var now = timeProvider.GetUtcNow();

        // Workflow nằm ở Domain (WorkItemWorkflow), handler không tự kiểm tra lại
        item.ChangeStatus(command.Status, now);

        db.WorkItemHistories.Add(new WorkItemHistory(item.TeamId, item.Id, WorkItemField.Status,
            from.ToString(), item.Status.ToString(), userId, now));

        // Báo cho người được giao và người tạo task (trừ người vừa đổi trạng thái)
        await notifications.StageTaskEventAsync(item, userId, NotificationType.TaskStatusChanged,
            item.Status.ToString(), [item.AssigneeId, item.ReporterId], cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return Unit.Value;
    }
}
