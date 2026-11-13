using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Projects.Commands.DeleteProject;

public sealed class DeleteProjectHandler(IAppDbContext db) : ICommandHandler<DeleteProjectCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Dự án", command.ProjectId);

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
