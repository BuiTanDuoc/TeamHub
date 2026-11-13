using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Tasks.Queries.GetTask;

public sealed record GetTaskQuery(Guid TeamId, Guid ProjectId, Guid TaskId) : IQuery<TaskDetailDto>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
