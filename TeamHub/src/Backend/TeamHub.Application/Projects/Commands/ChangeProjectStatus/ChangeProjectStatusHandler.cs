using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Projects.Commands.ChangeProjectStatus;

public sealed class ChangeProjectStatusHandler(IAppDbContext db) : ICommandHandler<ChangeProjectStatusCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangeProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Dự án", command.ProjectId);

        project.ChangeStatus(command.Status);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
