using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamHub.Domain.Users;

namespace TeamHub.Infrastructure.Identity;

/// <summary>Tạo tài khoản SystemAdmin đầu tiên khi hệ thống chưa có user nào và đã cấu hình Seed:Admin.</summary>
public sealed class IdentitySeeder(
    UserManager<AppUser> users,
    IOptions<SeedOptions> options,
    TimeProvider timeProvider,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var seed = options.Value;
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
            return;

        if (await users.Users.AnyAsync(cancellationToken))
            return;

        var admin = new AppUser
        {
            UserName = seed.Email,
            Email = seed.Email,
            EmailConfirmed = true,
            FullName = seed.FullName,
            IsSystemAdmin = true,
            CreatedAt = timeProvider.GetUtcNow()
        };

        var result = await users.CreateAsync(admin, seed.Password);
        if (result.Succeeded)
            logger.LogInformation("Đã tạo tài khoản SystemAdmin đầu tiên: {Email}", seed.Email);
        else
            logger.LogError("Không tạo được SystemAdmin: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
