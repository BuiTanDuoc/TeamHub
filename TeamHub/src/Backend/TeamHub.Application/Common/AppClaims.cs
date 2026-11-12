namespace TeamHub.Application.Common;

/// <summary>Tên claim trong JWT. Api tắt MapInboundClaims nên tên claim giữ nguyên như trong token.</summary>
public static class AppClaims
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Name = "name";

    /// <summary>Có mặt (giá trị "true") khi user là SystemAdmin.</summary>
    public const string SystemAdmin = "sysadmin";
}
