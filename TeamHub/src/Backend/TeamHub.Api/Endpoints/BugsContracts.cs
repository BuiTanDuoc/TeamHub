using TeamHub.Domain.Bugs;
using TeamHub.Domain.Common;

namespace TeamHub.Api.Endpoints;

// TeamId/ProjectId/BugId lấy từ route nên không nằm trong body.
public sealed record CreateBugRequest(
    string Title, BugSeverity Severity, Priority Priority, BugEnvironment Environment, string? AffectedVersion,
    string StepsToReproduce, string ExpectedResult, string ActualResult, string? DeviceInfo);

public sealed record UpdateBugRequest(
    string Title, BugSeverity Severity, Priority Priority, BugEnvironment Environment, string? AffectedVersion,
    string StepsToReproduce, string ExpectedResult, string ActualResult, string? DeviceInfo);

public sealed record ChangeBugStatusRequest(BugStatus Status, string? Note, string? FixedInVersion);

public sealed record AssignBugRequest(Guid AssigneeId);

public sealed record MarkBugDuplicateRequest(Guid OriginalBugId, string Note);
