using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Notifications;

public sealed class NotificationDispatcher(
    IAppDbContext db, INotificationPusher pusher, ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    // Khớp độ dài cột Message
    private const int MaxMessageLength = 500;

    private readonly List<Notification> _staged = [];

    public async Task StageTaskEventAsync(WorkItem task, Guid actorId, NotificationType type, string? detail,
        IEnumerable<Guid?> recipients, CancellationToken cancellationToken)
    {
        var targets = recipients.OfType<Guid>().Where(id => id != actorId).Distinct().ToList();
        if (targets.Count == 0)
            return;

        var actorName = await db.Users.Where(u => u.Id == actorId).Select(u => u.FullName)
            .SingleOrDefaultAsync(cancellationToken) ?? "Một thành viên";
        var projectKey = await db.Projects.Where(p => p.Id == task.ProjectId).Select(p => p.Key)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
        var code = $"{projectKey}-{task.Number}";

        var (title, message) = type switch
        {
            NotificationType.TaskAssigned =>
                ("Bạn được giao việc mới", $"{actorName} đã giao cho bạn {code}: {task.Title}"),
            NotificationType.TaskCommented =>
                ("Bình luận mới", $"{actorName} đã bình luận trong {code}: {task.Title}"),
            NotificationType.TaskStatusChanged =>
                ("Trạng thái task thay đổi", $"{actorName} chuyển {code}: {task.Title} sang {detail}"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        if (message.Length > MaxMessageLength)
            message = message[..MaxMessageLength];

        foreach (var recipientId in targets)
        {
            var notification = new Notification(recipientId, type, title, message, actorId,
                task.TeamId, task.ProjectId, task.Id);
            db.Notifications.Add(notification);
            _staged.Add(notification);
        }
    }

    public async Task PublishStagedAsync(CancellationToken cancellationToken)
    {
        var staged = _staged.ToList();
        _staged.Clear();

        foreach (var notification in staged)
        {
            try
            {
                await pusher.PushAsync(notification.RecipientId, notification.ToDto(), cancellationToken);
            }
            catch (Exception ex)
            {
                // Thông báo đã nằm trong DB, người dùng vẫn thấy khi mở app; chỉ mất bản đẩy realtime
                logger.LogWarning(ex, "Không đẩy được thông báo {NotificationId} tới người dùng {RecipientId}",
                    notification.Id, notification.RecipientId);
            }
        }
    }
}
