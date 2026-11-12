using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.ChangeTeamMemberRole;

public sealed class ChangeTeamMemberRoleHandler(IAppDbContext db)
    : ICommandHandler<ChangeTeamMemberRoleCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangeTeamMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var team = await db.Teams
            .Include(t => t.Members)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        // Domain chặn việc hạ vai trò TeamLead cuối cùng
        team.ChangeMemberRole(command.UserId, command.Role);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
