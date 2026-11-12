using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.UpdateTeam;

public sealed record UpdateTeamCommand(Guid TeamId, string Name, string? Description)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
