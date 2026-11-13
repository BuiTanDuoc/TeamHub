using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Common;
using TeamHub.Domain.Teams;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.CreateTask;

/// <summary>Tạo task/story/epic/sub-task; Number cấp từ bộ đếm của dự án.</summary>
public sealed record CreateTaskCommand(
    Guid TeamId, Guid ProjectId, WorkItemType Type, string Title, string? Description, Priority Priority,
    Guid? AssigneeId, Guid? ParentId, decimal? EstimateHours, DateOnly? DueDate)
    : ICommand<CreatedTaskDto>, IRequireTeamRole
{
    // Mọi vai trò trừ Viewer
    public TeamRole MinimumRole => TeamRole.Tester;
}
