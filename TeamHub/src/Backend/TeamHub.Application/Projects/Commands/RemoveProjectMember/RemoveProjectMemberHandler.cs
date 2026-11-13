using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberHandler(IAppDbContext db) : ICommandHandler<RemoveProjectMemberCommand, Unit>
{
    public async ValueTask<Unit> Handle(RemoveProjectMemberCommand command, CancellationToken cancellationToken)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Dự án", command.ProjectId);

        project.RemoveMember(command.UserId);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
