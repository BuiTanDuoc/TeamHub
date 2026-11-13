using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordHandler(IUserAdministration administration, ICurrentUser currentUser)
    : ICommandHandler<ChangeMyPasswordCommand, Unit>
{
    public async ValueTask<Unit> Handle(ChangeMyPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var result = await administration.ChangePasswordAsync(
            userId, command.CurrentPassword, command.NewPassword, cancellationToken);

        return result.Succeeded ? Unit.Value : throw new DomainException(result.ErrorMessage);
    }
}
