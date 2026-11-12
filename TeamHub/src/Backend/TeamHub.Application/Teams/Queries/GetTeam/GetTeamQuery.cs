using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Queries.GetTeam;

public sealed record GetTeamQuery(Guid TeamId) : IQuery<TeamDto>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
