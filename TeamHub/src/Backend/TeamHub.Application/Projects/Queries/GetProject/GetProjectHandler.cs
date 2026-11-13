using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Projects.Queries.GetProject;

public sealed class GetProjectHandler(IAppDbContext db) : IQueryHandler<GetProjectQuery, ProjectDto>
{
    public async ValueTask<ProjectDto> Handle(GetProjectQuery query, CancellationToken cancellationToken)
    {
        return await db.Projects
            .AsNoTracking()
            .Where(p => p.Id == query.ProjectId && p.TeamId == query.TeamId)
            .ToDtos()
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Dự án", query.ProjectId);
    }
}
