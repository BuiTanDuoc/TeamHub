using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.UpdateTeamMemberCapacity;

public sealed class UpdateTeamMemberCapacityHandler(IAppDbContext db)
    : ICommandHandler<UpdateTeamMemberCapacityCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateTeamMemberCapacityCommand command, CancellationToken cancellationToken)
    {
        var team = await db.Teams
            .Include(t => t.Members)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        team.SetMemberCapacity(command.UserId, command.WeeklyCapacityHours);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
