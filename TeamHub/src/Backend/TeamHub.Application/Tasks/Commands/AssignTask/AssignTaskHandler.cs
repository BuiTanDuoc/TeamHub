using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.AssignTask;

public sealed class AssignTaskHandler(
    IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, INotificationDispatcher notifications)
    : ICommandHandler<AssignTaskCommand, Unit>
{
    public async ValueTask<Unit> Handle(AssignTaskCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var item = await db.WorkItems
            .SingleOrDefaultAsync(w => w.Id == command.TaskId && w.TeamId == command.TeamId
                && w.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Task", command.TaskId);

        if (item.AssigneeId == command.AssigneeId)
            return Unit.Value;

        if (command.AssigneeId is { } assigneeId)
        {
            var isProjectMember = await db.ProjectMembers
                .AnyAsync(m => m.ProjectId == command.ProjectId && m.UserId == assigneeId, cancellationToken);
            if (!isProjectMember)
                throw new DomainException("Người được giao phải là thành viên của dự án.");
        }

        // Lịch sử lưu tên hiển thị tại thời điểm đổi để vẫn đọc được khi user đổi tên
        var ids = new[] { item.AssigneeId, command.AssigneeId }.OfType<Guid>().ToList();
        var names = await db.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);
        string? NameOf(Guid? id) => id is { } value ? names.GetValueOrDefault(value) : null;

        db.WorkItemHistories.Add(new WorkItemHistory(item.TeamId, item.Id, WorkItemField.Assignee,
            NameOf(item.AssigneeId), NameOf(command.AssigneeId), userId, timeProvider.GetUtcNow()));

        item.Assign(command.AssigneeId);

        // Báo cho người vừa được giao (người tự giao cho mình sẽ bị bỏ qua)
        if (command.AssigneeId is { } newAssigneeId)
        {
            await notifications.StageTaskEventAsync(item, userId, NotificationType.TaskAssigned, null,
                [newAssigneeId], cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return Unit.Value;
    }
}
