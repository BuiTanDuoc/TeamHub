using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(Guid TeamId, Guid UserId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
