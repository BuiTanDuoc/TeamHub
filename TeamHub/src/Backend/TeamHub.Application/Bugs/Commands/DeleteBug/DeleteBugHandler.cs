using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Bugs.Commands.DeleteBug;

public sealed class DeleteBugHandler(IAppDbContext db) : ICommandHandler<DeleteBugCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteBugCommand command, CancellationToken cancellationToken)
    {
        var bug = await db.Bugs
            .SingleOrDefaultAsync(b => b.Id == command.BugId && b.TeamId == command.TeamId
                && b.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Bug", command.BugId);

        db.Bugs.Remove(bug);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
