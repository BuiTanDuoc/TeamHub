using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications;

public sealed record NotificationDto(
    Guid Id, NotificationType Type, string Title, string Message,
    Guid? TeamId, Guid? ProjectId, Guid? WorkItemId, bool IsRead, DateTimeOffset CreatedAt);

public sealed record UnreadCountDto(int Count);
