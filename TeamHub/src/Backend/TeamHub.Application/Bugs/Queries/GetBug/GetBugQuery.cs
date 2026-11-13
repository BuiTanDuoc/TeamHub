using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Queries.GetBug;

public sealed record GetBugQuery(Guid TeamId, Guid ProjectId, Guid BugId) : IQuery<BugDetailDto>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
