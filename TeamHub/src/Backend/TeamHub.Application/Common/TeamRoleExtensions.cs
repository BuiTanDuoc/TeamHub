using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Common;

public static class TeamRoleExtensions
{
    /// <summary>
    /// Vai trò của người đang đăng nhập trong team, dùng cho quy tắc chi tiết ở Domain (ví dụ BugWorkflow).
    /// SystemAdmin không thuộc team được coi như TeamLead. AuthorizationBehavior đã chặn người ngoài team,
    /// nên ForbiddenException ở đây chỉ là lớp bảo vệ thứ hai.
    /// </summary>
    public static async Task<TeamRole> ResolveActorRoleAsync(
        this IAppDbContext db, ICurrentUser currentUser, Guid teamId, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var role = await db.TeamMembers.AsNoTracking()
            .Where(m => m.TeamId == teamId && m.UserId == userId)
            .Select(m => (TeamRole?)m.Role)
            .SingleOrDefaultAsync(cancellationToken);

        if (role is { } found)
            return found;

        return currentUser.IsSystemAdmin
            ? TeamRole.TeamLead
            : throw new ForbiddenException("Bạn không thuộc team này.");
    }
}
