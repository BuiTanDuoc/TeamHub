using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Api.Hubs;

/// <summary>
/// Kênh thông báo thời gian thực. Khi kết nối, client tự vào group của chính mình và của các team mình thuộc về.
/// Server đẩy tin qua IHubContext&lt;NotificationHub&gt; tới <see cref="HubGroups"/>.
/// </summary>
[Authorize]
public sealed class NotificationHub(IAppDbContext db) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetUserId();

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.User(userId));

        // TeamMembers không có query filter nên không phụ thuộc ICurrentUser trong ngữ cảnh hub.
        // Lưu ý: thay đổi thành viên khi đang kết nối chỉ có hiệu lực sau khi client kết nối lại.
        var teamIds = await db.TeamMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.TeamId)
            .ToListAsync(Context.ConnectionAborted);

        foreach (var teamId in teamIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.Team(teamId));

        await base.OnConnectedAsync();
    }
}
