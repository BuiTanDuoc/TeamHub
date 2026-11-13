using Mediator;

namespace TeamHub.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand<Unit>;
