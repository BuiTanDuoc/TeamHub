using FluentValidation;

namespace TeamHub.Application.Projects.Commands.ChangeProjectStatus;

public sealed class ChangeProjectStatusValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusValidator() => RuleFor(x => x.Status).IsInEnum();
}
