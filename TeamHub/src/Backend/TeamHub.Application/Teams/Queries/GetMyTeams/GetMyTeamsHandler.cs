using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Teams.Queries.GetMyTeams;

public sealed class GetMyTeamsHandler(IAppDbContext db) : IQueryHandler<GetMyTeamsQuery, PagedResult<TeamDto>>
{
    public async ValueTask<PagedResult<TeamDto>> Handle(GetMyTeamsQuery query, CancellationToken cancellationToken)
    {
        // Global query filter TeamScope đã giới hạn theo team của user; Id là khóa phụ để thứ tự ổn định
        return await db.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name).ThenBy(t => t.Id)
            .ToDtos()
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
