using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Notifications;
using TeamHub.Domain.Exceptions;
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

        // Bỏ chính tác giả; chỉ cho nhắc thành viên của team (không dùng @mention để báo cho người ngoài team)
        var mentioned = (command.MentionedUserIds ?? []).Where(id => id != authorId).Distinct().ToList();
        if (mentioned.Count > 0)
        {
            var memberCount = await db.TeamMembers
                .CountAsync(m => m.TeamId == command.TeamId && mentioned.Contains(m.UserId), cancellationToken);
            if (memberCount != mentioned.Count)
                throw new DomainException("Chỉ có thể nhắc đến thành viên của team.");
        }

        var comment = new WorkItemComment(command.TeamId, command.TaskId, authorId, command.Body);
        db.WorkItemComments.Add(comment);

        // Người được nhắc nhận thông báo "Mentioned"; người được giao/người tạo task nhận "TaskCommented",
        // trừ người đã được nhắc để một bình luận không báo hai lần cho cùng một người
        await notifications.StageTaskEventAsync(task, authorId, NotificationType.Mentioned, null,
            mentioned.Select(id => (Guid?)id), cancellationToken);
        await notifications.StageTaskEventAsync(task, authorId, NotificationType.TaskCommented, null,
            new Guid?[] { task.AssigneeId, task.ReporterId }.Where(id => id is null || !mentioned.Contains(id.Value)),
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await notifications.PublishStagedAsync(cancellationToken);

        return comment.Id;
    }
}
