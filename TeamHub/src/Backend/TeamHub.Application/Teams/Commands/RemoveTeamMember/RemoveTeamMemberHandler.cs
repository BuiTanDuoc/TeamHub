using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.RemoveTeamMember;

public sealed class RemoveTeamMemberHandler(IAppDbContext db) : ICommandHandler<RemoveTeamMemberCommand, Unit>
{
    public async ValueTask<Unit> Handle(RemoveTeamMemberCommand command, CancellationToken cancellationToken)
    {
        var team = await db.Teams
            .Include(t => t.Members)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        // Domain chặn việc xóa TeamLead cuối cùng
        team.RemoveMember(command.UserId);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
