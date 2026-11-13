using TeamHub.Domain.Bugs;
using TeamHub.Domain.Common;

namespace TeamHub.Application.Bugs;

/// <summary>Dùng cho danh sách bug.</summary>
public sealed record BugDto(
    Guid Id, Guid ProjectId, int Number, string Code, string Title, BugSeverity Severity, Priority Priority,
    BugStatus Status, BugEnvironment Environment, Guid? AssigneeId, string? AssigneeName,
    DateTimeOffset? SlaDueAt, DateTimeOffset CreatedAt);

public sealed record BugDetailDto(
    Guid Id, Guid ProjectId, int Number, string Code, string Title, BugSeverity Severity, Priority Priority,
    BugStatus Status, BugEnvironment Environment, string? AffectedVersion, string? FixedInVersion,
    string StepsToReproduce, string ExpectedResult, string ActualResult, string? DeviceInfo,
    Guid ReporterId, string ReporterName, Guid? AssigneeId, string? AssigneeName,
    Guid? LinkedWorkItemId, Guid? DuplicateOfBugId, DateTimeOffset? SlaDueAt,
    DateTimeOffset? ResolvedAt, DateTimeOffset? ClosedAt, int ReopenCount, DateTimeOffset CreatedAt)
{
    /// <summary>Trạng thái mà người đang xem được phép chuyển sang (theo workflow và vai trò của họ).</summary>
    public IReadOnlyList<BugStatus> AllowedTransitions { get; init; } = [];
}

public sealed record CreatedBugDto(Guid Id, int Number, string Code);

public sealed record BugStatusHistoryDto(
    BugStatus? FromStatus, BugStatus ToStatus, Guid ChangedById, string ChangedByName,
    DateTimeOffset ChangedAt, string? Note);
