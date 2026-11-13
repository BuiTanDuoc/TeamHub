using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Comments.Commands.DeleteComment;

public sealed class DeleteCommentHandler(IAppDbContext db, ICurrentUser currentUser) : ICommandHandler<DeleteCommentCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var comment = await db.WorkItemComments
            .SingleOrDefaultAsync(c => c.Id == command.CommentId && c.WorkItemId == command.TaskId
                && c.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Bình luận", command.CommentId);

        if (comment.AuthorId != userId && !currentUser.IsSystemAdmin)
        {
            var isLead = await db.TeamMembers.AnyAsync(m => m.TeamId == command.TeamId
                && m.UserId == userId && m.Role == TeamRole.TeamLead, cancellationToken);
            if (!isLead)
                throw new ForbiddenException("Chỉ tác giả hoặc TeamLead được xóa bình luận.");
        }

        db.WorkItemComments.Remove(comment);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
