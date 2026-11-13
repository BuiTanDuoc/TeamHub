using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications;

public static class NotificationMappings
{
    public static IQueryable<NotificationDto> ToDtos(this IQueryable<Notification> query) =>
        query.Select(n => new NotificationDto(
            n.Id, n.Type, n.Title, n.Message, n.TeamId, n.ProjectId, n.WorkItemId, n.IsRead, n.CreatedAt));

    /// <summary>Dùng cho entity vừa lưu (để đẩy realtime); CreatedAt đã được AuditInterceptor điền.</summary>
    public static NotificationDto ToDto(this Notification n) =>
        new(n.Id, n.Type, n.Title, n.Message, n.TeamId, n.ProjectId, n.WorkItemId, n.IsRead, n.CreatedAt);
}
