using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Notifications.Commands.PurgeOldNotifications;

namespace TeamHub.Api.Jobs;

/// <summary>Job Hangfire dọn thông báo cũ. Logic nằm ở PurgeOldNotificationsCommand; job chỉ tính mốc thời gian và gọi.</summary>
public sealed class NotificationCleanupJob(
    IMediator mediator, IOptions<NotificationCleanupOptions> options, TimeProvider timeProvider,
    ILogger<NotificationCleanupJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var now = timeProvider.GetUtcNow();

        var deleted = await mediator.Send(new PurgeOldNotificationsCommand(
            ReadBefore: now.AddDays(-settings.ReadRetentionDays),
            AnyBefore: now.AddDays(-settings.MaxRetentionDays),
            BatchSize: settings.BatchSize), cancellationToken);

        logger.LogInformation("Đã xóa {Count} thông báo cũ", deleted);
    }
}
