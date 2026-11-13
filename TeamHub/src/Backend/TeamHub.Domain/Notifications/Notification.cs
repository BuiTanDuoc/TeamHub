using TeamHub.Domain.Common;

namespace TeamHub.Domain.Notifications;

/// <summary>
/// Thông báo trong app gửi tới một người nhận. Không áp filter theo team: truy vấn luôn phải lọc theo
/// RecipientId. TeamId/ProjectId/WorkItemId chỉ để client điều hướng tới nội dung liên quan.
/// </summary>
public sealed class Notification : AuditableEntity
{
    private Notification() { } // EF Core

    public Notification(Guid recipientId, NotificationType type, string title, string message,
        Guid? actorId, Guid? teamId, Guid? projectId, Guid? workItemId, Guid? bugId = null)
    {
        RecipientId = recipientId;
        Type = type;
        Title = title;
        Message = message;
        ActorId = actorId;
        TeamId = teamId;
        ProjectId = projectId;
        WorkItemId = workItemId;
        BugId = bugId;
    }

    public Guid RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;

    /// <summary>Người gây ra sự kiện (null nếu do hệ thống).</summary>
    public Guid? ActorId { get; private set; }

    public Guid? TeamId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public Guid? WorkItemId { get; private set; }
    public Guid? BugId { get; private set; }

    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkRead(DateTimeOffset now)
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = now;
    }
}
