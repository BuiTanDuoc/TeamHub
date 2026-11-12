using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Queries.GetTeamMembers;

public sealed record GetTeamMembersQuery(Guid TeamId, int Page, int PageSize)
    : IQuery<PagedResult<TeamMemberDto>>, IPagedQuery, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
