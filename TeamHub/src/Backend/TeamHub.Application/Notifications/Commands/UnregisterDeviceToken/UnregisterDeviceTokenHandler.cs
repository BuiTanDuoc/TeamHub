using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Notifications.Commands.UnregisterDeviceToken;

public sealed class UnregisterDeviceTokenHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<UnregisterDeviceTokenCommand, Unit>
{
    public async ValueTask<Unit> Handle(UnregisterDeviceTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        // Chỉ gỡ token đang thuộc về mình; token không tồn tại cũng coi là đã gỡ (idempotent)
        await db.DeviceTokens
            .Where(t => t.Token == command.Token && t.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return Unit.Value;
    }
}
