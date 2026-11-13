using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Comments.Queries.GetComments;

/// <summary>Bình luận của task theo thứ tự thời gian (cũ nhất trước).</summary>
public sealed record GetCommentsQuery(Guid TeamId, Guid ProjectId, Guid TaskId, int Page, int PageSize)
    : IQuery<PagedResult<CommentDto>>, IPagedQuery, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
