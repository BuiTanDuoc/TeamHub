using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Teams.Queries.GetTeamMembers;

public sealed class GetTeamMembersHandler(IAppDbContext db)
    : IQueryHandler<GetTeamMembersQuery, PagedResult<TeamMemberDto>>
{
    public async ValueTask<PagedResult<TeamMemberDto>> Handle(
        GetTeamMembersQuery query, CancellationToken cancellationToken)
    {
        // TeamMembers không có filter theo team: AuthorizationBehavior đã xác nhận quyền, ở đây vẫn lọc rõ theo TeamId
        return await db.TeamMembers
            .AsNoTracking()
            .Where(m => m.TeamId == query.TeamId)
            .OrderBy(m => m.User.FullName).ThenBy(m => m.UserId)
            .ToDtos()
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
