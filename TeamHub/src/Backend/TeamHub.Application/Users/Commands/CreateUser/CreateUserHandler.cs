using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Users.Commands.CreateUser;

public sealed class CreateUserHandler(IUserAdministration administration) : ICommandHandler<CreateUserCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await administration.CreateUserAsync(
            command.Email, command.FullName, command.Password, command.IsSystemAdmin, cancellationToken);

        return result is { Succeeded: true, UserId: { } userId }
            ? userId
            : throw new DomainException(result.ErrorMessage);
    }
}
