using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Comments.Queries.GetComments;

public sealed class GetCommentsHandler(IAppDbContext db) : IQueryHandler<GetCommentsQuery, PagedResult<CommentDto>>
{
    public async ValueTask<PagedResult<CommentDto>> Handle(GetCommentsQuery query, CancellationToken cancellationToken)
    {
        var taskExists = await db.WorkItems.AnyAsync(w => w.Id == query.TaskId
            && w.TeamId == query.TeamId && w.ProjectId == query.ProjectId, cancellationToken);
        if (!taskExists)
            throw new NotFoundException("Task", query.TaskId);

        // UpdatedAt chỉ đổi khi sửa nội dung (xóa mềm thì bình luận không còn hiển thị)
        return await db.WorkItemComments.AsNoTracking()
            .Where(c => c.WorkItemId == query.TaskId && c.TeamId == query.TeamId)
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
            .Select(c => new CommentDto(c.Id, c.AuthorId, c.Author.FullName, c.Author.AvatarUrl, c.Body, c.CreatedAt, c.UpdatedAt))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
