using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Notifications;

public sealed class NotificationDispatcher(
    IAppDbContext db, INotificationPusher pusher, IPushSender pushSender, ILogger<NotificationDispatcher> logger)
    : INotificationDispatcher
{
    // Khớp độ dài cột Message
    private const int MaxMessageLength = 500;

    /// <summary>InApp: lưu DB và đẩy realtime. Push: gửi tới điện thoại (có thể bật mà không lưu trong app).</summary>
    private sealed record StagedNotification(Notification Notification, bool InApp, bool Push);

    private readonly List<StagedNotification> _staged = [];

    public Task StageTaskEventAsync(WorkItem task, Guid actorId, NotificationType type, string? detail,
        IEnumerable<Guid?> recipients, CancellationToken cancellationToken) =>
        StageAsync(task.TeamId, task.ProjectId, task.Number, task.Title, task.Id, null,
            actorId, type, detail, recipients, cancellationToken);

    public Task StageBugEventAsync(Bug bug, Guid actorId, NotificationType type, string? detail,
        IEnumerable<Guid?> recipients, CancellationToken cancellationToken) =>
        StageAsync(bug.TeamId, bug.ProjectId, bug.Number, bug.Title, null, bug.Id,
            actorId, type, detail, recipients, cancellationToken);

    private async Task StageAsync(Guid teamId, Guid projectId, int number, string subject, Guid? workItemId, Guid? bugId,
        Guid actorId, NotificationType type, string? detail, IEnumerable<Guid?> recipients,
        CancellationToken cancellationToken)
    {
        var targets = recipients.OfType<Guid>().Where(id => id != actorId).Distinct().ToList();
        if (targets.Count == 0)
            return;

        // Không có cài đặt nghĩa là bật tất cả kênh
        var preferences = await db.NotificationPreferences.AsNoTracking()
            .Where(p => p.Type == type && targets.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        var actorName = await db.Users.Where(u => u.Id == actorId).Select(u => u.FullName)
            .SingleOrDefaultAsync(cancellationToken) ?? "Một thành viên";
        var projectKey = await db.Projects.Where(p => p.Id == projectId).Select(p => p.Key)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
        var code = $"{projectKey}-{number}";

        var (title, message) = type switch
        {
            NotificationType.TaskAssigned =>
                ("Bạn được giao việc mới", $"{actorName} đã giao cho bạn {code}: {subject}"),
            NotificationType.TaskCommented =>
                ("Bình luận mới", $"{actorName} đã bình luận trong {code}: {subject}"),
            NotificationType.TaskStatusChanged =>
                ("Trạng thái task thay đổi", $"{actorName} chuyển {code}: {subject} sang {detail}"),
            NotificationType.BugAssigned =>
                ("Bạn được giao bug mới", $"{actorName} đã giao cho bạn {code}: {subject}"),
            NotificationType.BugStatusChanged =>
                ("Trạng thái bug thay đổi", $"{actorName} chuyển {code}: {subject} sang {detail}"),
            NotificationType.Mentioned =>
                ("Bạn được nhắc đến", $"{actorName} đã nhắc đến bạn trong {code}: {subject}"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        if (message.Length > MaxMessageLength)
            message = message[..MaxMessageLength];

        foreach (var recipientId in targets)
        {
            var inApp = true;
            var push = true;
            if (preferences.TryGetValue(recipientId, out var preference))
            {
                inApp = preference.InAppEnabled;
                push = preference.PushEnabled;
            }

            if (!inApp && !push)
                continue;

            var notification = new Notification(recipientId, type, title, message, actorId,
                teamId, projectId, workItemId, bugId);

            if (inApp)
                db.Notifications.Add(notification);

            _staged.Add(new StagedNotification(notification, inApp, push));
        }
    }

    public async Task PublishStagedAsync(CancellationToken cancellationToken)
    {
        var staged = _staged.ToList();
        _staged.Clear();

        foreach (var item in staged.Where(s => s.InApp))
        {
            try
            {
                await pusher.PushAsync(item.Notification.RecipientId, item.Notification.ToDto(), cancellationToken);
            }
            catch (Exception ex)
            {
                // Thông báo đã nằm trong DB, người dùng vẫn thấy khi mở app; chỉ mất bản đẩy realtime
                logger.LogWarning(ex, "Không đẩy realtime được thông báo {NotificationId} tới người dùng {RecipientId}",
                    item.Notification.Id, item.Notification.RecipientId);
            }
        }

        var pushItems = staged.Where(s => s.Push).ToList();
        if (pushItems.Count == 0)
            return;

        try
        {
            await SendMobilePushAsync(pushItems, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được push tới điện thoại cho {Count} thông báo", pushItems.Count);
        }
    }

    private async Task SendMobilePushAsync(List<StagedNotification> pushItems, CancellationToken cancellationToken)
    {
        var userIds = pushItems.Select(s => s.Notification.RecipientId).Distinct().ToList();
        var tokens = await db.DeviceTokens.AsNoTracking()
            .Where(t => userIds.Contains(t.UserId))
            .Select(t => new { t.UserId, t.Token })
            .ToListAsync(cancellationToken);

        var messages = new List<PushMessage>();
        foreach (var item in pushItems)
        {
            var n = item.Notification;
            var data = new Dictionary<string, string> { ["type"] = n.Type.ToString() };
            if (item.InApp) data["notificationId"] = n.Id.ToString();
            if (n.TeamId is { } teamId) data["teamId"] = teamId.ToString();
            if (n.ProjectId is { } projectId) data["projectId"] = projectId.ToString();
            if (n.WorkItemId is { } workItemId) data["workItemId"] = workItemId.ToString();
            if (n.BugId is { } bugId) data["bugId"] = bugId.ToString();

            messages.AddRange(tokens.Where(t => t.UserId == n.RecipientId)
                .Select(t => new PushMessage(t.Token, n.Title, n.Message, data)));
        }

        if (messages.Count == 0)
            return;

        var invalidTokens = await pushSender.SendAsync(messages, cancellationToken);
        if (invalidTokens.Count > 0)
        {
            // Thiết bị đã gỡ app hoặc token hết hạn: xóa để lần sau không gửi lại
            await db.DeviceTokens.Where(t => invalidTokens.Contains(t.Token)).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
