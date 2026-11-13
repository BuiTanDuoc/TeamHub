using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Bugs.Queries.GetBugHistory;

public sealed class GetBugHistoryHandler(IAppDbContext db)
    : IQueryHandler<GetBugHistoryQuery, PagedResult<BugStatusHistoryDto>>
{
    public async ValueTask<PagedResult<BugStatusHistoryDto>> Handle(GetBugHistoryQuery query, CancellationToken cancellationToken)
    {
        var exists = await db.Bugs.AnyAsync(b => b.Id == query.BugId && b.TeamId == query.TeamId
            && b.ProjectId == query.ProjectId, cancellationToken);
        if (!exists)
            throw new NotFoundException("Bug", query.BugId);

        return await db.BugStatusHistories.AsNoTracking()
            .Where(h => h.BugId == query.BugId)
            .OrderByDescending(h => h.ChangedAt).ThenBy(h => h.Id)
            .Select(h => new BugStatusHistoryDto(h.FromStatus, h.ToStatus, h.ChangedById, h.ChangedBy.FullName, h.ChangedAt, h.Note))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
