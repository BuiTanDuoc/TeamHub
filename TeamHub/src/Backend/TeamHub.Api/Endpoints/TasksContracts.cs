using TeamHub.Domain.Common;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Api.Endpoints;

// TeamId/ProjectId/TaskId lấy từ route nên không nằm trong body.
public sealed record CreateTaskRequest(
    WorkItemType Type, string Title, string? Description, Priority Priority,
    Guid? AssigneeId, Guid? ParentId, decimal? EstimateHours, DateOnly? DueDate);

public sealed record UpdateTaskRequest(
    string Title, string? Description, Priority Priority, decimal? EstimateHours, DateOnly? DueDate);

public sealed record ChangeTaskStatusRequest(WorkItemStatus Status);

public sealed record AssignTaskRequest(Guid? AssigneeId);
