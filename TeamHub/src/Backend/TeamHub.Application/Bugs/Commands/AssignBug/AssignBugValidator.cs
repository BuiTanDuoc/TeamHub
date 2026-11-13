using FluentValidation;

namespace TeamHub.Application.Bugs.Commands.AssignBug;

public sealed class AssignBugValidator : AbstractValidator<AssignBugCommand>
{
    public AssignBugValidator() => RuleFor(x => x.AssigneeId).NotEmpty();
}
