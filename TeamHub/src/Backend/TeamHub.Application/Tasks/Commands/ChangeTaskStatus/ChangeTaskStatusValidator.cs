using FluentValidation;

namespace TeamHub.Application.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusValidator() => RuleFor(x => x.Status).IsInEnum();
}
