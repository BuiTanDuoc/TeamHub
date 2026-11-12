using Microsoft.AspNetCore.Identity;

namespace TeamHub.Domain.Users;

public class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        Id = Guid.CreateVersion7();
    }

    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    /// <summary>Quản trị toàn hệ thống (không phụ thuộc vai trò trong từng team).</summary>
    public bool IsSystemAdmin { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
