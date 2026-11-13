using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Common;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Queries.GetKanbanBoard;

public sealed class GetKanbanBoardHandler(IAppDbContext db, IOptions<KanbanOptions> options)
    : IQueryHandler<GetKanbanBoardQuery, KanbanBoardDto>
{
    public async ValueTask<KanbanBoardDto> Handle(GetKanbanBoardQuery query, CancellationToken cancellationToken)
    {
        var projectExists = await db.Projects
            .AnyAsync(p => p.Id == query.ProjectId && p.TeamId == query.TeamId, cancellationToken);
        if (!projectExists)
            throw new NotFoundException("Dự án", query.ProjectId);

        var items = db.WorkItems.AsNoTracking()
            .Where(w => w.TeamId == query.TeamId && w.ProjectId == query.ProjectId && w.Type != WorkItemType.Epic);

        if (query.AssigneeId is { } assigneeId)
            items = items.Where(w => w.AssigneeId == assigneeId);

        var counts = await items
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var maxCards = options.Value.MaxCardsPerColumn;
        var columns = new List<KanbanColumnDto>();

        // DbContext không an toàn đa luồng nên chạy lần lượt từng cột
        foreach (var status in Enum.GetValues<WorkItemStatus>())
        {
            var total = counts.GetValueOrDefault(status);
            IReadOnlyList<TaskDto> cards = [];

            if (total > 0)
            {
                var column = items.Where(w => w.Status == status);

                // Cột Done: hoàn thành gần nhất trước. Các cột khác: ưu tiên cao trước, rồi theo số thứ tự.
                // Priority lưu dạng chuỗi nên sắp xếp bằng CASE chứ không sort theo chữ.
                var ordered = status == WorkItemStatus.Done
                    ? column.OrderByDescending(w => w.CompletedAt).ThenBy(w => w.Number)
                    : column
                        .OrderByDescending(w => w.Priority == Priority.Urgent ? 3
                            : w.Priority == Priority.High ? 2
                            : w.Priority == Priority.Medium ? 1 : 0)
                        .ThenBy(w => w.Number);

                cards = await ordered.Take(maxCards).ToCards().ToListAsync(cancellationToken);
            }

            columns.Add(new KanbanColumnDto(status, total, cards));
        }

        return new KanbanBoardDto(query.ProjectId, columns);
    }
}
