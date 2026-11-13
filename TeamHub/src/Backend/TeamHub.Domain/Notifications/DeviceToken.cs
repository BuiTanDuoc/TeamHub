using TeamHub.Domain.Common;

namespace TeamHub.Domain.Notifications;

/// <summary>Expo push token của một thiết bị. Một token chỉ thuộc một người dùng tại một thời điểm.</summary>
public sealed class DeviceToken : AuditableEntity
{
    private DeviceToken() { } // EF Core

    public DeviceToken(Guid userId, string token, DevicePlatform platform, DateTimeOffset now)
    {
        UserId = userId;
        Token = token;
        Platform = platform;
        LastSeenAt = now;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DevicePlatform Platform { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }

    /// <summary>Gán token cho người đang đăng nhập (máy được dùng lại bởi tài khoản khác) và làm mới thời điểm dùng.</summary>
    public void Claim(Guid userId, DevicePlatform platform, DateTimeOffset now)
    {
        UserId = userId;
        Platform = platform;
        LastSeenAt = now;
    }
}
