using TeamHub.Domain.Common;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks;

/// <summary>Dùng cho danh sách và thẻ trên Kanban board.</summary>
public sealed record TaskDto(
    Guid Id, Guid ProjectId, int Number, string Code, WorkItemType Type, Guid? ParentId, string Title,
    WorkItemStatus Status, Priority Priority, Guid? AssigneeId, string? AssigneeName,
    decimal? EstimateHours, DateOnly? DueDate, DateTimeOffset CreatedAt);

public sealed record TaskDetailDto(
    Guid Id, Guid ProjectId, int Number, string Code, WorkItemType Type, Guid? ParentId, string Title,
    string? Description, WorkItemStatus Status, Priority Priority,
    Guid ReporterId, string ReporterName, Guid? AssigneeId, string? AssigneeName, Guid? ReviewerId,
    decimal? EstimateHours, DateOnly? DueDate, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt)
{
    /// <summary>Các cột mà thẻ này được phép kéo sang (theo WorkItemWorkflow).</summary>
    public IReadOnlyList<WorkItemStatus> AllowedTransitions { get; init; } = [];
}

public sealed record CreatedTaskDto(Guid Id, int Number, string Code);

public sealed record TaskHistoryDto(
    WorkItemField Field, string? OldValue, string? NewValue, Guid ChangedById, string ChangedByName, DateTimeOffset ChangedAt);

public sealed record KanbanColumnDto(WorkItemStatus Status, int TotalCount, IReadOnlyList<TaskDto> Cards);

public sealed record KanbanBoardDto(Guid ProjectId, IReadOnlyList<KanbanColumnDto> Columns);
