using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Tasks.Queries.GetKanbanBoard;

/// <summary>Bảng Kanban của dự án: mỗi trạng thái một cột (Epic không hiển thị trên bảng).</summary>
public sealed record GetKanbanBoardQuery(Guid TeamId, Guid ProjectId, Guid? AssigneeId)
    : IQuery<KanbanBoardDto>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
