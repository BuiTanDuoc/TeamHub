using Microsoft.AspNetCore.SignalR;
using TeamHub.Application.Common;

namespace TeamHub.Api.Auth;

/// <summary>
/// SignalR mặc định lấy UserIdentifier từ ClaimTypes.NameIdentifier; vì đã tắt MapInboundClaims
/// nên phải chỉ định rõ claim "sub".
/// </summary>
public sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User.FindFirst(AppClaims.Subject)?.Value;
}
