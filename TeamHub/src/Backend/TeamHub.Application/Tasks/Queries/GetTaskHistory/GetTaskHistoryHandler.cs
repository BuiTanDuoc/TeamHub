using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Tasks.Queries.GetTaskHistory;

public sealed class GetTaskHistoryHandler(IAppDbContext db)
    : IQueryHandler<GetTaskHistoryQuery, PagedResult<TaskHistoryDto>>
{
    public async ValueTask<PagedResult<TaskHistoryDto>> Handle(GetTaskHistoryQuery query, CancellationToken cancellationToken)
    {
        // Xác nhận task thuộc đúng team/dự án trước khi trả lịch sử
        var exists = await db.WorkItems
            .AnyAsync(w => w.Id == query.TaskId && w.TeamId == query.TeamId && w.ProjectId == query.ProjectId, cancellationToken);
        if (!exists)
            throw new NotFoundException("Task", query.TaskId);

        return await db.WorkItemHistories.AsNoTracking()
            .Where(h => h.WorkItemId == query.TaskId)
            .OrderByDescending(h => h.ChangedAt).ThenBy(h => h.Id)
            .Select(h => new TaskHistoryDto(h.Field, h.OldValue, h.NewValue, h.ChangedById, h.ChangedBy.FullName, h.ChangedAt))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
