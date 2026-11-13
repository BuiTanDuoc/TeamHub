using FluentValidation;

namespace TeamHub.Application.Projects.Commands.AddProjectMember;

public sealed class AddProjectMemberValidator : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberValidator() => RuleFor(x => x.UserId).NotEmpty();
}
