using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Comments.Commands.AddComment;

public sealed class AddCommentHandler(IAppDbContext db, ICurrentUser currentUser) : ICommandHandler<AddCommentCommand, Guid>
{
    public async ValueTask<Guid> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        var authorId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var taskExists = await db.WorkItems.AnyAsync(w => w.Id == command.TaskId
            && w.TeamId == command.TeamId && w.ProjectId == command.ProjectId, cancellationToken);
        if (!taskExists)
            throw new NotFoundException("Task", command.TaskId);

        var comment = new WorkItemComment(command.TeamId, command.TaskId, authorId, command.Body);
        db.WorkItemComments.Add(comment);
        await db.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
