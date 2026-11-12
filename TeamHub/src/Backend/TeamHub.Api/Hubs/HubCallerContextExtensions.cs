using Microsoft.AspNetCore.SignalR;

namespace TeamHub.Api.Hubs;

public static class HubCallerContextExtensions
{
    public static Guid GetUserId(this HubCallerContext context) =>
        Guid.TryParse(context.UserIdentifier, out var id)
            ? id
            : throw new HubException("Chưa đăng nhập.");
}
