using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Commands.RemoveProjectMember;

public sealed record RemoveProjectMemberCommand(Guid TeamId, Guid ProjectId, Guid UserId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
