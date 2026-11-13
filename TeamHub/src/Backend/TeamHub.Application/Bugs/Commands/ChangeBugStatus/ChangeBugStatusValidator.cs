using FluentValidation;

namespace TeamHub.Application.Bugs.Commands.ChangeBugStatus;

public sealed class ChangeBugStatusValidator : AbstractValidator<ChangeBugStatusCommand>
{
    public ChangeBugStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(2000);
        RuleFor(x => x.FixedInVersion).MaximumLength(50);
    }
}
