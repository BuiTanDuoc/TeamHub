using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Queries.GetProject;

public sealed record GetProjectQuery(Guid TeamId, Guid ProjectId) : IQuery<ProjectDto>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
