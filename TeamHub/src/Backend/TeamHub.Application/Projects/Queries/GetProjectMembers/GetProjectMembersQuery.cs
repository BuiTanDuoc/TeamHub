using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Queries.GetProjectMembers;

public sealed record GetProjectMembersQuery(Guid TeamId, Guid ProjectId, int Page, int PageSize)
    : IQuery<PagedResult<ProjectMemberDto>>, IPagedQuery, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
