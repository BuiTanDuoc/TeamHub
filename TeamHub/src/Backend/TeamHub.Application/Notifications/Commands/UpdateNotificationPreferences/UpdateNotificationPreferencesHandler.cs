using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications.Commands.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<UpdateNotificationPreferencesCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateNotificationPreferencesCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var types = command.Items.Select(i => i.Type).ToList();
        var existing = await db.NotificationPreferences
            .Where(p => p.UserId == userId && types.Contains(p.Type))
            .ToDictionaryAsync(p => p.Type, cancellationToken);

        foreach (var item in command.Items)
        {
            if (existing.TryGetValue(item.Type, out var preference))
            {
                preference.Update(item.InAppEnabled, item.PushEnabled);
            }
            else if (!(item.InAppEnabled && item.PushEnabled))
            {
                // Bật tất cả là mặc định nên không cần lưu bản ghi
                db.NotificationPreferences.Add(
                    new NotificationPreference(userId, item.Type, item.InAppEnabled, item.PushEnabled));
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
