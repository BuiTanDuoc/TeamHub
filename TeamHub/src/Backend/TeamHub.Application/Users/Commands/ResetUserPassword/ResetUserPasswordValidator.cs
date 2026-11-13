using FluentValidation;

namespace TeamHub.Application.Users.Commands.ResetUserPassword;

public sealed class ResetUserPasswordValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordValidator() => RuleFor(x => x.NewPassword).NotEmpty().MaximumLength(128);
}
