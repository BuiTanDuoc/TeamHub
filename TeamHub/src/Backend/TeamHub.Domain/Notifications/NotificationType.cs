namespace TeamHub.Domain.Notifications;

public enum NotificationType
{
    TaskAssigned = 0,
    TaskCommented = 1,
    TaskStatusChanged = 2,
    BugAssigned = 3,
    BugStatusChanged = 4,

    /// <summary>Được @nhắc đến trong bình luận.</summary>
    Mentioned = 5
}
