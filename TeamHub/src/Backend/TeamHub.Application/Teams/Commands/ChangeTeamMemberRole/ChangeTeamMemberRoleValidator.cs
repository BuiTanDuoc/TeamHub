using FluentValidation;

namespace TeamHub.Application.Teams.Commands.ChangeTeamMemberRole;

public sealed class ChangeTeamMemberRoleValidator : AbstractValidator<ChangeTeamMemberRoleCommand>
{
    public ChangeTeamMemberRoleValidator() => RuleFor(x => x.Role).IsInEnum();
}
