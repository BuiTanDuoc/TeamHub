using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Teams.Commands.AddTeamMember;

public sealed class AddTeamMemberHandler(IAppDbContext db, TimeProvider timeProvider)
    : ICommandHandler<AddTeamMemberCommand, Guid>
{
    public async ValueTask<Guid> Handle(AddTeamMemberCommand command, CancellationToken cancellationToken)
    {
        // Team.AddMember cần danh sách thành viên đã được load
        var team = await db.Teams
            .Include(t => t.Members)
            .SingleOrDefaultAsync(t => t.Id == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);

        // Identity chuẩn hóa email bằng upper-invariant; tìm theo NormalizedEmail để dùng được index
        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var userId = await db.Users
            .Where(u => u.NormalizedEmail == normalizedEmail && u.IsActive)
            .Select(u => (Guid?)u.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Người dùng", command.Email);

        var now = timeProvider.GetUtcNow();
        if (command.WeeklyCapacityHours is { } hours)
            team.AddMember(userId, command.Role, now, hours);
        else
            team.AddMember(userId, command.Role, now);

        await db.SaveChangesAsync(cancellationToken);

        return userId;
    }
}
