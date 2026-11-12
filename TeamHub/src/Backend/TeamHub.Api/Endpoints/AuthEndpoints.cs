using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Api.Endpoints;

public static class RateLimitPolicies
{
    public const string Auth = "auth";
}

public sealed record MyTeamDto(Guid TeamId, string Name, string Code, TeamRole Role);

public sealed record MeResponse(AuthUserDto User, IReadOnlyList<MyTeamDto> Teams);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        // Các endpoint đăng nhập/làm mới cho phép ẩn danh và giới hạn tần suất theo IP
        group.MapPost("/login", async (LoginRequest request, HttpContext http, IAuthService auth, CancellationToken ct) =>
                TypedResults.Ok(await auth.LoginAsync(request.Email, request.Password, DeviceOf(http), ct)))
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/google", async (GoogleLoginRequest request, HttpContext http, IAuthService auth, CancellationToken ct) =>
                TypedResults.Ok(await auth.LoginWithGoogleAsync(request.IdToken, DeviceOf(http), ct)))
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group.MapPost("/refresh", async (RefreshRequest request, HttpContext http, IAuthService auth, CancellationToken ct) =>
                TypedResults.Ok(await auth.RefreshAsync(request.RefreshToken, DeviceOf(http), ct)))
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth);

        // Logout chỉ cần refresh token (access token có thể đã hết hạn)
        group.MapPost("/logout", async (RefreshRequest request, IAuthService auth, CancellationToken ct) =>
            {
                await auth.LogoutAsync(request.RefreshToken, ct);
                return TypedResults.NoContent();
            })
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth);

        // Yêu cầu đăng nhập nhờ fallback policy
        group.MapGet("/me", async (ICurrentUser currentUser, IAppDbContext db, CancellationToken ct) =>
        {
            var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

            var user = await db.Users
                .Where(u => u.Id == userId && u.IsActive)
                .Select(u => new AuthUserDto(u.Id, u.Email ?? string.Empty, u.FullName, u.AvatarUrl, u.IsSystemAdmin))
                .SingleOrDefaultAsync(ct)
                ?? throw new AuthenticationFailedException("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

            // TeamMembers không có filter theo team nên phải lọc rõ theo UserId
            var teams = await db.TeamMembers
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Team.Name)
                .Select(m => new MyTeamDto(m.TeamId, m.Team.Name, m.Team.Code, m.Role))
                .ToListAsync(ct);

            return TypedResults.Ok(new MeResponse(user, teams));
        });

        return app;
    }

    private static string DeviceOf(HttpContext http) => http.Request.Headers.UserAgent.ToString();
}
