using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;

namespace TeamHub.Application.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<UpdateMyProfileCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("Người dùng", userId);

        user.FullName = command.FullName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(command.AvatarUrl) ? null : command.AvatarUrl.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
