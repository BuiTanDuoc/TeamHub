namespace TeamHub.Application.Notifications;

/// <summary>Đẩy thông báo realtime tới một người dùng. Triển khai ở Api bằng SignalR.</summary>
public interface INotificationPusher
{
    Task PushAsync(Guid recipientId, NotificationDto notification, CancellationToken cancellationToken);
}
