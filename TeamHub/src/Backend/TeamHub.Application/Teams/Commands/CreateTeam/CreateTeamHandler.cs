using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.CreateTeam;

public sealed class CreateTeamHandler(IAppDbContext db, TimeProvider timeProvider)
    : ICommandHandler<CreateTeamCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var code = command.Code.Trim().ToUpperInvariant();

        if (await db.Teams.AnyAsync(t => t.Code == code, cancellationToken))
            throw new DomainException("Mã team đã tồn tại.");

        var leadExists = await db.Users
            .AnyAsync(u => u.Id == command.LeadUserId && u.IsActive, cancellationToken);
        if (!leadExists)
            throw new NotFoundException("Người dùng", command.LeadUserId);

        // Team luôn phải có ít nhất một TeamLead ngay từ đầu (domain cũng chặn việc hạ/xóa lead cuối cùng)
        var team = new Team(command.Name, code, command.Description);
        team.AddMember(command.LeadUserId, TeamRole.TeamLead, timeProvider.GetUtcNow());

        db.Teams.Add(team);
        await db.SaveChangesAsync(cancellationToken);

        return team.Id;
    }
}
