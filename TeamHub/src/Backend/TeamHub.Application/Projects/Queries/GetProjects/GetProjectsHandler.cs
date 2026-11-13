using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Projects.Queries.GetProjects;

public sealed class GetProjectsHandler(IAppDbContext db) : IQueryHandler<GetProjectsQuery, PagedResult<ProjectDto>>
{
    public async ValueTask<PagedResult<ProjectDto>> Handle(GetProjectsQuery query, CancellationToken cancellationToken)
    {
        var projects = db.Projects.AsNoTracking().Where(p => p.TeamId == query.TeamId);

        if (query.Status is { } status)
            projects = projects.Where(p => p.Status == status);

        return await projects
            .OrderBy(p => p.Name).ThenBy(p => p.Id)
            .ToDtos()
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
