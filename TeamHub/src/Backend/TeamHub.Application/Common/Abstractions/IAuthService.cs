namespace TeamHub.Application.Common.Abstractions;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, string? device, CancellationToken cancellationToken);

    /// <summary>Đăng nhập bằng Google ID token do client (web/mobile) lấy được từ Google.</summary>
    Task<AuthResult> LoginWithGoogleAsync(string idToken, string? device, CancellationToken cancellationToken);

    /// <summary>Đổi refresh token lấy cặp token mới (xoay vòng, token cũ bị thu hồi).</summary>
    Task<AuthResult> RefreshAsync(string refreshToken, string? device, CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}

public sealed record AuthUserDto(Guid Id, string Email, string FullName, string? AvatarUrl, bool IsSystemAdmin);

public sealed record AuthResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthUserDto User);
