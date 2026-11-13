using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Projects.Commands.UpdateProject;

public sealed class UpdateProjectHandler(IAppDbContext db) : ICommandHandler<UpdateProjectCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Dự án", command.ProjectId);

        project.Update(command.Name, command.Description, command.StartDate, command.EndDate, command.UseSprints);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
