using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Common;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Commands.CreateBug;

/// <summary>Báo bug mới (trạng thái New). Number cấp từ cùng bộ đếm với task; SLA tính từ Bugs:Sla theo severity.</summary>
public sealed record CreateBugCommand(
    Guid TeamId, Guid ProjectId, string Title, BugSeverity Severity, Priority Priority, BugEnvironment Environment,
    string? AffectedVersion, string StepsToReproduce, string ExpectedResult, string ActualResult, string? DeviceInfo)
    : ICommand<CreatedBugDto>, IRequireTeamRole
{
    // Mọi vai trò trừ Viewer đều được báo bug
    public TeamRole MinimumRole => TeamRole.Tester;
}
