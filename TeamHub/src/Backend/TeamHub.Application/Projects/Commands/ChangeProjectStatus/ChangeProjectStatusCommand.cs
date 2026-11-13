using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Projects;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Commands.ChangeProjectStatus;

public sealed record ChangeProjectStatusCommand(Guid TeamId, Guid ProjectId, ProjectStatus Status)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
