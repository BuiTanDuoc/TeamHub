using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.UpdateTeam;

public sealed class UpdateTeamHandler(IAppDbContext db) : ICommandHandler<UpdateTeamCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await db.Teams.SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        team.Update(command.Name, command.Description);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
