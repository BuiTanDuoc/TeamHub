using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Comments.Commands.EditComment;

public sealed class EditCommentHandler(IAppDbContext db, ICurrentUser currentUser) : ICommandHandler<EditCommentCommand, Unit>
{
    public async ValueTask<Unit> Handle(EditCommentCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var comment = await db.WorkItemComments
            .SingleOrDefaultAsync(c => c.Id == command.CommentId && c.WorkItemId == command.TaskId
                && c.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Bình luận", command.CommentId);

        // Đây là quy tắc sở hữu dữ liệu (không phải vai trò trong team) nên kiểm tra tại handler
        if (comment.AuthorId != userId)
            throw new ForbiddenException("Chỉ tác giả được sửa bình luận.");

        comment.Edit(command.Body);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
