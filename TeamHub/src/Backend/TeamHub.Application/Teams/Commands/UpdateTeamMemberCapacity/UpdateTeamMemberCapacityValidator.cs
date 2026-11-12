using FluentValidation;

namespace TeamHub.Application.Teams.Commands.UpdateTeamMemberCapacity;

public sealed class UpdateTeamMemberCapacityValidator : AbstractValidator<UpdateTeamMemberCapacityCommand>
{
    // 168 = số giờ trong một tuần
    public UpdateTeamMemberCapacityValidator() => RuleFor(x => x.WeeklyCapacityHours).InclusiveBetween(0m, 168m);
}
