using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Users.Commands.SetUserActive;

public sealed class SetUserActiveHandler(IAppDbContext db, ICurrentUser currentUser, IUserAdministration administration)
    : ICommandHandler<SetUserActiveCommand, Unit>
{
    public async ValueTask<Unit> Handle(SetUserActiveCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Người dùng", command.UserId);

        if (user.IsActive == command.IsActive)
            return Unit.Value;

        if (!command.IsActive)
        {
            if (user.Id == currentUser.UserId)
                throw new DomainException("Bạn không thể tự vô hiệu hóa tài khoản của mình.");

            // Luôn phải còn ít nhất một SystemAdmin đang hoạt động, nếu không không ai quản trị được hệ thống
            if (user.IsSystemAdmin && !await db.Users.AnyAsync(
                    u => u.Id != user.Id && u.IsSystemAdmin && u.IsActive, cancellationToken))
                throw new DomainException("Phải còn ít nhất một SystemAdmin đang hoạt động.");
        }

        user.IsActive = command.IsActive;
        await db.SaveChangesAsync(cancellationToken);

        if (!command.IsActive)
        {
            // Hết quyền làm mới phiên và không còn nhận push của hệ thống
            await administration.RevokeSessionsAsync(user.Id, cancellationToken);
            await db.DeviceTokens.Where(t => t.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
