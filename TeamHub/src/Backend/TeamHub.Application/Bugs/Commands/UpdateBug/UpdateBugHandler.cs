using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Bugs;

namespace TeamHub.Application.Bugs.Commands.UpdateBug;

public sealed class UpdateBugHandler(IAppDbContext db, IOptions<BugOptions> options) : ICommandHandler<UpdateBugCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateBugCommand command, CancellationToken cancellationToken)
    {
        var bug = await db.Bugs
            .SingleOrDefaultAsync(b => b.Id == command.BugId && b.TeamId == command.TeamId
                && b.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Bug", command.BugId);

        var oldSeverity = bug.Severity;
        bug.Update(command.Title, command.Severity, command.Priority, command.Environment, command.AffectedVersion,
            command.StepsToReproduce, command.ExpectedResult, command.ActualResult, command.DeviceInfo);

        // Hạn SLA tính từ lúc tạo bug; bug đã kết thúc thì không còn tính SLA
        if (oldSeverity != bug.Severity && !BugWorkflow.IsFinal(bug.Status))
        {
            bug.SetSlaDueAt(options.Value.SlaFor(bug.Severity) is { } sla ? bug.CreatedAt + sla : null);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
