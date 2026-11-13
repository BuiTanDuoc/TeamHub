using Hangfire;
using Microsoft.Extensions.Options;

namespace TeamHub.Api.Jobs;

public static class HangfireExtensions
{
    /// <summary>
    /// Đăng ký Hangfire (lưu job ở SQL Server) và các job của TeamHub. Đặt Hangfire:Enabled = false
    /// để tắt hẳn (ví dụ môi trường test không có SQL Server).
    /// </summary>
    public static IServiceCollection AddTeamHubJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotificationCleanupOptions>()
            .Bind(configuration.GetSection(NotificationCleanupOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Cron)
                    && o.ReadRetentionDays > 0
                    && o.MaxRetentionDays >= o.ReadRetentionDays
                    && o.BatchSize is > 0 and <= 2000,
                "Notifications:Cleanup không hợp lệ: cần Cron, ReadRetentionDays > 0, MaxRetentionDays >= ReadRetentionDays, BatchSize trong 1-2000.")
            .ValidateOnStart();

        // Hangfire tạo job qua DI, mỗi lần chạy trong một scope riêng
        services.AddScoped<NotificationCleanupJob>();

        if (!configuration.GetValue("Hangfire:Enabled", true))
            return services;

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:Default.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString));
        services.AddHangfireServer();

        return services;
    }

    /// <summary>Đăng ký/cập nhật lịch chạy. Gọi sau InitializeDatabaseAsync để cơ sở dữ liệu đã sẵn sàng.</summary>
    public static WebApplication UseTeamHubRecurringJobs(this WebApplication app)
    {
        if (!app.Configuration.GetValue("Hangfire:Enabled", true))
            return app;

        var cleanup = app.Services.GetRequiredService<IOptions<NotificationCleanupOptions>>().Value;

        app.Services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<NotificationCleanupJob>(
            NotificationCleanupOptions.JobId,
            job => job.RunAsync(CancellationToken.None), // Hangfire thay bằng token hủy của job
            cleanup.Cron,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        return app;
    }
}
