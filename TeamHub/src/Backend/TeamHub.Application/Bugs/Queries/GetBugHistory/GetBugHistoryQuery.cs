using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Queries.GetBugHistory;

public sealed record GetBugHistoryQuery(Guid TeamId, Guid ProjectId, Guid BugId, int Page, int PageSize)
    : IQuery<PagedResult<BugStatusHistoryDto>>, IPagedQuery, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
