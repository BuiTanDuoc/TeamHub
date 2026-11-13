using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Queries.GetTask;

public sealed class GetTaskHandler(IAppDbContext db) : IQueryHandler<GetTaskQuery, TaskDetailDto>
{
    public async ValueTask<TaskDetailDto> Handle(GetTaskQuery query, CancellationToken cancellationToken)
    {
        var task = await db.WorkItems.AsNoTracking()
            .Where(w => w.Id == query.TaskId && w.TeamId == query.TeamId && w.ProjectId == query.ProjectId)
            .ToDetails()
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Task", query.TaskId);

        // Danh sách cột được phép kéo sang lấy từ cùng workflow với lúc đổi trạng thái
        var allowed = Enum.GetValues<WorkItemStatus>()
            .Where(s => WorkItemWorkflow.CanTransition(task.Status, s))
            .ToList();

        return task with { AllowedTransitions = allowed };
    }
}
