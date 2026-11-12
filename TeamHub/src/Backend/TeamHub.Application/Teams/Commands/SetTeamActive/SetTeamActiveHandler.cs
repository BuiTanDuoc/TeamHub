using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.SetTeamActive;

public sealed class SetTeamActiveHandler(IAppDbContext db) : ICommandHandler<SetTeamActiveCommand, Unit>
{
    public async ValueTask<Unit> Handle(SetTeamActiveCommand command, CancellationToken cancellationToken)
    {
        var team = await db.Teams.SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        team.SetActive(command.IsActive);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
