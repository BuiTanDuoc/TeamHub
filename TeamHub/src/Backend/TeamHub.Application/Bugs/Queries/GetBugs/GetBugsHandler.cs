using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Bugs.Queries.GetBugs;

public sealed class GetBugsHandler(IAppDbContext db) : IQueryHandler<GetBugsQuery, CursorPagedResult<BugDto>>
{
    public async ValueTask<CursorPagedResult<BugDto>> Handle(GetBugsQuery query, CancellationToken cancellationToken)
    {
        var bugs = db.Bugs.AsNoTracking()
            .Where(b => b.TeamId == query.TeamId && b.ProjectId == query.ProjectId);

        if (query.Status is { } status) bugs = bugs.Where(b => b.Status == status);
        if (query.Severity is { } severity) bugs = bugs.Where(b => b.Severity == severity);
        if (query.AssigneeId is { } assigneeId) bugs = bugs.Where(b => b.AssigneeId == assigneeId);

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search)) bugs = bugs.Where(b => b.Title.Contains(search));

        if (query.Cursor is { } cursor) bugs = bugs.Where(b => b.Number < cursor);

        // Lấy dư một phần tử để biết còn trang sau hay không
        var page = await bugs
            .OrderByDescending(b => b.Number)
            .Take(query.PageSize + 1)
            .ToCards()
            .ToListAsync(cancellationToken);

        var hasMore = page.Count > query.PageSize;
        var items = hasMore ? page.Take(query.PageSize).ToList() : page;

        return new CursorPagedResult<BugDto>(items, hasMore ? items[^1].Number : null);
    }
}
