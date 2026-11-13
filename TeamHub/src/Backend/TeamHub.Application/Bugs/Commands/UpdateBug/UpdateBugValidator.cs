using FluentValidation;

namespace TeamHub.Application.Bugs.Commands.UpdateBug;

public sealed class UpdateBugValidator : AbstractValidator<UpdateBugCommand>
{
    public UpdateBugValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Environment).IsInEnum();
        RuleFor(x => x.AffectedVersion).MaximumLength(50);
        RuleFor(x => x.StepsToReproduce).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.ExpectedResult).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ActualResult).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.DeviceInfo).MaximumLength(500);
    }
}
