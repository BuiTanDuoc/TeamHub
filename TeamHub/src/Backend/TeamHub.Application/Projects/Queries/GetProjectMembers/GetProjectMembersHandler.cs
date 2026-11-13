using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Projects.Queries.GetProjectMembers;

public sealed class GetProjectMembersHandler(IAppDbContext db)
    : IQueryHandler<GetProjectMembersQuery, PagedResult<ProjectMemberDto>>
{
    public async ValueTask<PagedResult<ProjectMemberDto>> Handle(
        GetProjectMembersQuery query, CancellationToken cancellationToken)
    {
        return await db.ProjectMembers
            .AsNoTracking()
            .Where(m => m.TeamId == query.TeamId && m.ProjectId == query.ProjectId)
            .OrderBy(m => m.User.FullName).ThenBy(m => m.UserId)
            .ToDtos()
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
