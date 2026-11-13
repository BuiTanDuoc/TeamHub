using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications.Commands.RegisterDeviceToken;

public sealed class RegisterDeviceTokenHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<RegisterDeviceTokenCommand, Unit>
{
    public async ValueTask<Unit> Handle(RegisterDeviceTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");
        var now = timeProvider.GetUtcNow();

        var device = await db.DeviceTokens.SingleOrDefaultAsync(t => t.Token == command.Token, cancellationToken);
        if (device is null)
        {
            db.DeviceTokens.Add(new DeviceToken(userId, command.Token, command.Platform, now));
        }
        else
        {
            // Token đã có: có thể máy được đăng nhập bằng tài khoản khác, chuyển sang người dùng hiện tại
            device.Claim(userId, command.Platform, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
