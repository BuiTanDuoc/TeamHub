using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Infrastructure.Identity;

public sealed record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? Name, string? Picture);

public interface IGoogleTokenValidator
{
    Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken);
}

/// <summary>Xác thực Google ID token: chữ ký, hạn dùng, issuer và audience (client id của mình).</summary>
public sealed class GoogleTokenValidator(IOptions<GoogleAuthOptions> options) : IGoogleTokenValidator
{
    public async Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientIds = options.Value.ClientIds;
        if (clientIds.Length == 0)
            throw new AuthenticationFailedException("Đăng nhập Google chưa được cấu hình.");

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = clientIds });

            return new GoogleIdentity(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException)
        {
            throw new AuthenticationFailedException("Google token không hợp lệ.");
        }
    }
}
