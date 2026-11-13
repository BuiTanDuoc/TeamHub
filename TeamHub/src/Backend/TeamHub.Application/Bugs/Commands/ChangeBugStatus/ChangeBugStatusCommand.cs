using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Commands.ChangeBugStatus;

/// <summary>
/// Chuyển trạng thái bug. Workflow, vai trò được phép và lý do bắt buộc do BugWorkflow ở Domain kiểm tra:
/// sai chuyển → 409, sai vai trò → 403, thiếu lý do → 400.
/// </summary>
public sealed record ChangeBugStatusCommand(
    Guid TeamId, Guid ProjectId, Guid BugId, BugStatus Status, string? Note, string? FixedInVersion)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
