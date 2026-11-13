using Mediator;

namespace TeamHub.Application.Notifications.Queries.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery : IQuery<UnreadCountDto>;
