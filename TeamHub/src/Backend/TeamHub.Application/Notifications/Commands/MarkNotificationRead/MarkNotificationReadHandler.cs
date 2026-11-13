using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<MarkNotificationReadCommand, Unit>
{
    public async ValueTask<Unit> Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        // Thông báo không có filter theo team nên bắt buộc lọc theo người nhận;
        // thông báo của người khác trả NotFound để không lộ việc nó tồn tại
        var notification = await db.Notifications
            .SingleOrDefaultAsync(n => n.Id == command.NotificationId && n.RecipientId == userId, cancellationToken)
            ?? throw new NotFoundException("Thông báo", command.NotificationId);

        notification.MarkRead(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
