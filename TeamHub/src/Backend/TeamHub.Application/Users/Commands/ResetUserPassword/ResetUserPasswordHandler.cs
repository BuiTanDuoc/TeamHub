using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Users.Commands.ResetUserPassword;

public sealed class ResetUserPasswordHandler(IUserAdministration administration)
    : ICommandHandler<ResetUserPasswordCommand, Unit>
{
    public async ValueTask<Unit> Handle(ResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await administration.ResetPasswordAsync(command.UserId, command.NewPassword, cancellationToken);
        return result.Succeeded ? Unit.Value : throw new DomainException(result.ErrorMessage);
    }
}
