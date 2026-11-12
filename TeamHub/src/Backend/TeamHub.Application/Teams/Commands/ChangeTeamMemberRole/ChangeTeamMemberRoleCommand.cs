using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.ChangeTeamMemberRole;

public sealed record ChangeTeamMemberRoleCommand(Guid TeamId, Guid UserId, TeamRole Role)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
