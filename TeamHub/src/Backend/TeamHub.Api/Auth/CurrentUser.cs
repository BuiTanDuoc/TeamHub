using System.Security.Claims;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Api.Auth;

/// <summary>
/// Đọc user hiện tại từ JWT của request. Ngoài HTTP request (job nền, khởi động app) không có
/// HttpContext nên UserId = null và IsSystemAdmin = false: không bao giờ mặc định "toàn quyền".
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(AppClaims.Subject), out var id) ? id : null;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsSystemAdmin => Principal?.HasClaim(AppClaims.SystemAdmin, "true") == true;
}
