using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Teams;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Queries.GetTasks;

/// <summary>
/// Danh sách task của dự án, mới nhất trước. Keyset pagination theo Number (duy nhất trong dự án):
/// Cursor là Number của phần tử cuối trang trước.
/// </summary>
public sealed record GetTasksQuery(
    Guid TeamId, Guid ProjectId, WorkItemStatus? Status, WorkItemType? Type, Guid? AssigneeId,
    Guid? ParentId, string? Search, int? Cursor, int PageSize)
    : IQuery<CursorPagedResult<TaskDto>>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
