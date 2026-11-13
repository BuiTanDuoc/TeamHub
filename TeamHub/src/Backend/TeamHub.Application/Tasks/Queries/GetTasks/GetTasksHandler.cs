using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Tasks.Queries.GetTasks;

public sealed class GetTasksHandler(IAppDbContext db) : IQueryHandler<GetTasksQuery, CursorPagedResult<TaskDto>>
{
    public async ValueTask<CursorPagedResult<TaskDto>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var tasks = db.WorkItems.AsNoTracking()
            .Where(w => w.TeamId == query.TeamId && w.ProjectId == query.ProjectId);

        if (query.Status is { } status) tasks = tasks.Where(w => w.Status == status);
        if (query.Type is { } type) tasks = tasks.Where(w => w.Type == type);
        if (query.AssigneeId is { } assigneeId) tasks = tasks.Where(w => w.AssigneeId == assigneeId);
        if (query.ParentId is { } parentId) tasks = tasks.Where(w => w.ParentId == parentId);

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search)) tasks = tasks.Where(w => w.Title.Contains(search));

        if (query.Cursor is { } cursor) tasks = tasks.Where(w => w.Number < cursor);

        // Lấy dư một phần tử để biết còn trang sau hay không
        var page = await tasks
            .OrderByDescending(w => w.Number)
            .Take(query.PageSize + 1)
            .ToCards()
            .ToListAsync(cancellationToken);

        var hasMore = page.Count > query.PageSize;
        var items = hasMore ? page.Take(query.PageSize).ToList() : page;

        return new CursorPagedResult<TaskDto>(items, hasMore ? items[^1].Number : null);
    }
}
