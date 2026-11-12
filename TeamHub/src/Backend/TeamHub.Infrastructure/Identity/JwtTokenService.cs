using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TeamHub.Application.Common;
using TeamHub.Domain.Users;

namespace TeamHub.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(AppUser user)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(AppClaims.Subject, user.Id.ToString()),
            new(AppClaims.Email, user.Email ?? string.Empty),
            new(AppClaims.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        // Claim này tồn tại đến khi access token hết hạn (mặc định 15 phút) dù quyền admin bị thu hồi.
        if (user.IsSystemAdmin)
            claims.Add(new Claim(AppClaims.SystemAdmin, "true"));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }

    /// <summary>Refresh token ngẫu nhiên 512 bit, mã hóa base64url.</summary>
    public static string GenerateRefreshToken() =>
        Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

    /// <summary>Hash SHA-256 (hex, 64 ký tự) để lưu DB. Token đã đủ ngẫu nhiên nên không cần salt.</summary>
    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
