using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Users;
using TeamHub.Infrastructure.Persistence;

namespace TeamHub.Infrastructure.Identity;

public sealed class AuthService(
    UserManager<AppUser> users,
    AppDbContext db,
    JwtTokenService tokens,
    IGoogleTokenValidator googleValidator,
    IOptions<JwtOptions> jwtOptions,
    IOptions<GoogleAuthOptions> googleOptions,
    TimeProvider timeProvider) : IAuthService
{
    private const string GoogleProvider = "Google";

    public async Task<AuthResult> LoginAsync(string email, string password, string? device, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            throw InvalidCredentials();

        if (await users.IsLockedOutAsync(user))
            throw new AuthenticationFailedException("Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau.");

        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            throw InvalidCredentials();
        }

        await users.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, device, cancellationToken);
    }

    public async Task<AuthResult> LoginWithGoogleAsync(string idToken, string? device, CancellationToken cancellationToken)
    {
        var google = await googleValidator.ValidateAsync(idToken, cancellationToken);
        var options = googleOptions.Value;

        // Chỉ liên kết/tạo tài khoản theo email đã được Google xác minh
        if (!google.EmailVerified)
            throw new AuthenticationFailedException("Email Google chưa được xác minh.");

        if (options.AllowedDomains.Length > 0 &&
            !options.AllowedDomains.Contains(EmailDomain(google.Email), StringComparer.OrdinalIgnoreCase))
            throw new AuthenticationFailedException("Email không thuộc tên miền được phép.");

        var user = await users.FindByLoginAsync(GoogleProvider, google.Subject);
        var alreadyLinked = user is not null;
        user ??= await users.FindByEmailAsync(google.Email);

        if (user is null)
        {
            if (!options.AutoCreateUsers)
                throw new AuthenticationFailedException("Tài khoản chưa được tạo. Vui lòng liên hệ quản trị viên.");

            user = new AppUser
            {
                UserName = google.Email,
                Email = google.Email,
                EmailConfirmed = true,
                FullName = google.Name ?? google.Email,
                AvatarUrl = google.Picture,
                CreatedAt = timeProvider.GetUtcNow()
            };
            EnsureSucceeded(await users.CreateAsync(user));
        }

        if (!user.IsActive)
            throw new AuthenticationFailedException("Tài khoản đã bị vô hiệu hóa.");

        if (!alreadyLinked)
            EnsureSucceeded(await users.AddLoginAsync(user, new UserLoginInfo(GoogleProvider, google.Subject, GoogleProvider)));

        return await IssueTokensAsync(user, device, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, string? device, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var hash = JwtTokenService.Hash(refreshToken);

        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken)
                     ?? throw InvalidSession();

        if (stored.RevokedAt is not null)
        {
            // Token đã bị thu hồi mà vẫn được dùng lại: nghi ngờ bị đánh cắp, thu hồi toàn bộ phiên của user
            await db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTimeOffset?)now), cancellationToken);
            throw InvalidSession();
        }

        if (stored.ExpiresAt <= now)
            throw InvalidSession();

        var user = await users.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            stored.Revoke(now);
            await db.SaveChangesAsync(cancellationToken);
            throw InvalidSession();
        }

        // Xoay vòng: thu hồi token cũ, cấp token mới
        var newRaw = JwtTokenService.GenerateRefreshToken();
        var newHash = JwtTokenService.Hash(newRaw);
        stored.Revoke(now, newHash);

        return await IssueTokensAsync(user, device, cancellationToken, newRaw, newHash);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = JwtTokenService.Hash(refreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is { RevokedAt: null })
        {
            stored.Revoke(timeProvider.GetUtcNow());
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResult> IssueTokensAsync(
        AppUser user, string? device, CancellationToken cancellationToken,
        string? rawRefreshToken = null, string? refreshTokenHash = null)
    {
        var now = timeProvider.GetUtcNow();
        var (accessToken, accessExpiresAt) = tokens.CreateAccessToken(user);

        rawRefreshToken ??= JwtTokenService.GenerateRefreshToken();
        refreshTokenHash ??= JwtTokenService.Hash(rawRefreshToken);
        var refreshExpiresAt = now.AddDays(jwtOptions.Value.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken(user.Id, refreshTokenHash, now, refreshExpiresAt, Truncate(device, 200)));
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken, accessExpiresAt, rawRefreshToken, refreshExpiresAt,
            new AuthUserDto(user.Id, user.Email ?? string.Empty, user.FullName, user.AvatarUrl, user.IsSystemAdmin));
    }

    private static AuthenticationFailedException InvalidCredentials() => new("Email hoặc mật khẩu không đúng.");

    private static AuthenticationFailedException InvalidSession() => new("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");

    private static string EmailDomain(string email) => email[(email.LastIndexOf('@') + 1)..];

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
