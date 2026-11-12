using TeamHub.Domain.Common;

namespace TeamHub.Domain.Users;

/// <summary>Refresh token xoay vòng. Chỉ lưu hash (SHA-256), không lưu token gốc.</summary>
public sealed class RefreshToken : BaseEntity
{
    private RefreshToken() { } // EF Core

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt, string? device)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        Device = device;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? Device { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Hash của token thay thế khi xoay vòng (để truy vết chuỗi token).</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now, string? replacedByTokenHash = null)
    {
        RevokedAt = now;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
