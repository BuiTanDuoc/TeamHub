using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Projects;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Queries.GetProjects;

/// <summary>Danh sách dự án của team, lọc tùy chọn theo trạng thái.</summary>
public sealed record GetProjectsQuery(Guid TeamId, ProjectStatus? Status, int Page, int PageSize)
    : IQuery<PagedResult<ProjectDto>>, IPagedQuery, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
