using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Comments.Commands.AddComment;

public sealed class AddCommentHandler(IAppDbContext db, ICurrentUser currentUser, INotificationDispatcher notifications)
    : ICommandHandler<AddCommentCommand, Guid>
{
    public async ValueTask<Guid> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        var authorId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var task = await db.WorkItems.AsNoTracking()
            .SingleOrDefaultAsync(w => w.Id == command.TaskId
                && w.TeamId == command.TeamId && w.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Task", command.TaskId);

        var comment = new WorkItemComment(command.TeamId, command.TaskId, authorId, command.Body);
        db.WorkItemComments.Add(comment);

        // Báo cho người được giao và người tạo task (trừ chính tác giả bình luận)
        await notifications.StageTaskEventAsync(task, authorId, NotificationType.TaskCommented, null,
            [task.AssigneeId, task.ReporterId], cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return comment.Id;
    }
}
