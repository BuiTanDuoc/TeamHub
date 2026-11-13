using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Commands.AssignBug;

/// <summary>Giao bug cho thành viên dự án. Bug Confirmed/Reopened tự chuyển sang Assigned (xem Bug.Assign).</summary>
public sealed record AssignBugCommand(Guid TeamId, Guid ProjectId, Guid BugId, Guid AssigneeId)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
