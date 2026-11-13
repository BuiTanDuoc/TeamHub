using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Tasks.Queries.GetTaskHistory;

public sealed record GetTaskHistoryQuery(Guid TeamId, Guid ProjectId, Guid TaskId, int Page, int PageSize)
    : IQuery<PagedResult<TaskHistoryDto>>, IPagedQuery, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
