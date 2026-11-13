using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Users.Queries.GetUser;

public sealed class GetUserHandler(IAppDbContext db) : IQueryHandler<GetUserQuery, UserDetailDto>
{
    public async ValueTask<UserDetailDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => new { u.Id, Email = u.Email ?? string.Empty, u.FullName, u.AvatarUrl, u.IsActive, u.IsSystemAdmin, u.CreatedAt })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Người dùng", query.UserId);

        var teams = await (
                from m in db.TeamMembers.AsNoTracking()
                join t in db.Teams.AsNoTracking() on m.TeamId equals t.Id
                where m.UserId == query.UserId
                orderby t.Name
                select new UserTeamDto(t.Id, t.Name, m.Role))
            .ToListAsync(cancellationToken);

        return new UserDetailDto(user.Id, user.Email, user.FullName, user.AvatarUrl,
            user.IsActive, user.IsSystemAdmin, user.CreatedAt, teams);
    }
}
