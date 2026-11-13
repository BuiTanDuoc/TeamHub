using FluentValidation;

namespace TeamHub.Application.Bugs.Commands.MarkBugDuplicate;

public sealed class MarkBugDuplicateValidator : AbstractValidator<MarkBugDuplicateCommand>
{
    public MarkBugDuplicateValidator()
    {
        RuleFor(x => x.OriginalBugId).NotEmpty();
        RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
    }
}
