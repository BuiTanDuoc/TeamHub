using System.ComponentModel.DataAnnotations;

namespace TeamHub.Api.Endpoints;

public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}

public sealed record GoogleLoginRequest
{
    /// <summary>ID token (JWT) mà Google trả về cho client.</summary>
    [Required, MaxLength(4096)]
    public string IdToken { get; init; } = string.Empty;
}

public sealed record RefreshRequest
{
    [Required, MaxLength(256)]
    public string RefreshToken { get; init; } = string.Empty;
}
