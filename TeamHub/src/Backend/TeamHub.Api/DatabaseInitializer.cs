using Microsoft.EntityFrameworkCore;
using TeamHub.Infrastructure.Identity;
using TeamHub.Infrastructure.Persistence;

namespace TeamHub.Api;

public static class DatabaseInitializer
{
    /// <summary>
    /// Tùy chọn tự migrate (Database:MigrateOnStartup, bật ở Development) rồi tạo SystemAdmin đầu tiên
    /// nếu có cấu hình Seed:Admin. Production nên chạy migration trong pipeline triển khai.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync(CancellationToken.None);
    }
}
