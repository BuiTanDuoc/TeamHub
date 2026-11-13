using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Common;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Commands.UpdateBug;

/// <summary>Sửa nội dung bug. Đổi severity sẽ tính lại hạn SLA (nếu bug chưa kết thúc).</summary>
public sealed record UpdateBugCommand(
    Guid TeamId, Guid ProjectId, Guid BugId, string Title, BugSeverity Severity, Priority Priority,
    BugEnvironment Environment, string? AffectedVersion, string StepsToReproduce, string ExpectedResult,
    string ActualResult, string? DeviceInfo)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
