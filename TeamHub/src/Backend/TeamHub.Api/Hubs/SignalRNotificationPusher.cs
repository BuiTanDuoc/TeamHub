using Microsoft.AspNetCore.SignalR;
using TeamHub.Application.Notifications;

namespace TeamHub.Api.Hubs;

/// <summary>Đẩy thông báo tới group riêng của người nhận; client lắng nghe sự kiện "notification".</summary>
public sealed class SignalRNotificationPusher(IHubContext<NotificationHub> hub) : INotificationPusher
{
    public const string EventName = "notification";

    public Task PushAsync(Guid recipientId, NotificationDto notification, CancellationToken cancellationToken) =>
        hub.Clients.Group(HubGroups.User(recipientId)).SendAsync(EventName, notification, cancellationToken);
}
