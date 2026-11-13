using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications.Queries.GetNotificationPreferences;

public sealed class GetNotificationPreferencesHandler(IAppDbContext db, ICurrentUser currentUser)
    : IQueryHandler<GetNotificationPreferencesQuery, IReadOnlyList<NotificationPreferenceDto>>
{
    public async ValueTask<IReadOnlyList<NotificationPreferenceDto>> Handle(
        GetNotificationPreferencesQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var saved = await db.NotificationPreferences.AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.Type, cancellationToken);

        // Trả đủ mọi loại để client luôn dựng được màn hình cài đặt, loại chưa lưu thì mặc định bật
        return Enum.GetValues<NotificationType>()
            .Select(type => saved.TryGetValue(type, out var p)
                ? new NotificationPreferenceDto(type, p.InAppEnabled, p.PushEnabled)
                : new NotificationPreferenceDto(type, true, true))
            .ToList();
    }
}
