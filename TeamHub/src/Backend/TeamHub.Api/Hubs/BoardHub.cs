using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Common;

namespace TeamHub.Api.Hubs;

/// <summary>Kênh cập nhật board của một dự án: client gọi JoinProject khi mở board, LeaveProject khi rời.</summary>
[Authorize]
public sealed class BoardHub(IAppDbContext db) : Hub
{
    public async Task JoinProject(Guid projectId)
    {
        var userId = Context.GetUserId();
        var isSystemAdmin = Context.User?.HasClaim(AppClaims.SystemAdmin, "true") == true;

        // Kiểm tra quyền tường minh theo userId của kết nối (không dựa vào ICurrentUser/HttpContext trong hub):
        // bỏ riêng filter TeamScope, vẫn giữ filter SoftDelete.
        var allowed = isSystemAdmin || await db.Projects
            .IgnoreQueryFilters([QueryFilterNames.TeamScope])
            .Where(p => p.Id == projectId)
            .AnyAsync(p => db.TeamMembers.Any(m => m.TeamId == p.TeamId && m.UserId == userId),
                Context.ConnectionAborted);

        if (!allowed)
            throw new HubException("Bạn không có quyền xem dự án này.");

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.Project(projectId));
    }

    public Task LeaveProject(Guid projectId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, HubGroups.Project(projectId));
}
