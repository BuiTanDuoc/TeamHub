using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks;

/// <summary>Mapping thủ công dạng projection để EF chỉ select các cột cần thiết.</summary>
public static class TaskMappings
{
    // Mã hiển thị {Project.Key}-{Number}
    public static IQueryable<TaskDto> ToCards(this IQueryable<WorkItem> query) =>
        query.Select(w => new TaskDto(
            w.Id, w.ProjectId, w.Number, w.Project.Key + "-" + w.Number.ToString(), w.Type, w.ParentId, w.Title,
            w.Status, w.Priority, w.AssigneeId, w.Assignee != null ? w.Assignee.FullName : null,
            w.EstimateHours, w.DueDate, w.CreatedAt));

    public static IQueryable<TaskDetailDto> ToDetails(this IQueryable<WorkItem> query) =>
        query.Select(w => new TaskDetailDto(
            w.Id, w.ProjectId, w.Number, w.Project.Key + "-" + w.Number.ToString(), w.Type, w.ParentId, w.Title,
            w.Description, w.Status, w.Priority,
            w.ReporterId, w.Reporter.FullName, w.AssigneeId, w.Assignee != null ? w.Assignee.FullName : null, w.ReviewerId,
            w.EstimateHours, w.DueDate, w.CompletedAt, w.CreatedAt));
}
