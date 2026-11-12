using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Queries.GetTeam;

public sealed class GetTeamHandler(IAppDbContext db) : IQueryHandler<GetTeamQuery, TeamDto>
{
    public async ValueTask<TeamDto> Handle(GetTeamQuery query, CancellationToken cancellationToken)
    {
        return await db.Teams
            .AsNoTracking()
            .Where(t => t.Id == query.TeamId)
            .ToDtos()
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Team", query.TeamId);
    }
}
