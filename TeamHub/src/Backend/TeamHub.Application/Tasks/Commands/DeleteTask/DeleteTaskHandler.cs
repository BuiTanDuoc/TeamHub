using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskHandler(IAppDbContext db) : ICommandHandler<DeleteTaskCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var item = await db.WorkItems
            .SingleOrDefaultAsync(w => w.Id == command.TaskId && w.TeamId == command.TeamId
                && w.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Task", command.TaskId);

        // Query filter SoftDelete đã loại các item con đã xóa
        if (await db.WorkItems.AnyAsync(w => w.ParentId == item.Id, cancellationToken))
            throw new DomainException("Task còn item con, hãy xóa hoặc chuyển item con trước.");

        db.WorkItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
