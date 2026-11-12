using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.LeaveTeam;

public sealed class LeaveTeamHandler(IAppDbContext db, ICurrentUser currentUser) : ICommandHandler<LeaveTeamCommand, Unit>
{
    public async ValueTask<Unit> Handle(LeaveTeamCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var team = await db.Teams
            .Include(t => t.Members)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        team.RemoveMember(userId);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
