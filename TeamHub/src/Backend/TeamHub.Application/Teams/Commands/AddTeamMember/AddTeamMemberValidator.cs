using FluentValidation;

namespace TeamHub.Application.Teams.Commands.AddTeamMember;

public sealed class AddTeamMemberValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Role).IsInEnum();

        // 168 = số giờ trong một tuần
        RuleFor(x => x.WeeklyCapacityHours).InclusiveBetween(0m, 168m)
            .When(x => x.WeeklyCapacityHours.HasValue);
    }
}
