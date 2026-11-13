using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Users.Commands.SetUserSystemAdmin;

public sealed class SetUserSystemAdminHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<SetUserSystemAdminCommand, Unit>
{
    public async ValueTask<Unit> Handle(SetUserSystemAdminCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Người dùng", command.UserId);

        if (user.IsSystemAdmin == command.IsSystemAdmin)
            return Unit.Value;

        if (command.IsSystemAdmin)
        {
            if (!user.IsActive)
                throw new DomainException("Phải kích hoạt tài khoản trước khi cấp quyền SystemAdmin.");
        }
        else
        {
            if (user.Id == currentUser.UserId)
                throw new DomainException("Bạn không thể tự thu hồi quyền SystemAdmin của mình.");

            if (!await db.Users.AnyAsync(u => u.Id != user.Id && u.IsSystemAdmin && u.IsActive, cancellationToken))
                throw new DomainException("Phải còn ít nhất một SystemAdmin đang hoạt động.");
        }

        user.IsSystemAdmin = command.IsSystemAdmin;
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
