using FluentValidation;

namespace TeamHub.Application.Teams.Commands.CreateTeam;

public sealed class CreateTeamValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Mã team chỉ gồm chữ, số, '-' và '_'.");
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.LeadUserId).NotEmpty();
    }
}
