using FluentValidation;

namespace TeamHub.Application.Tasks.Commands.UpdateTaskDetails;

public sealed class UpdateTaskDetailsValidator : AbstractValidator<UpdateTaskDetailsCommand>
{
    public UpdateTaskDetailsValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(20000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.EstimateHours).InclusiveBetween(0m, 99999m).When(x => x.EstimateHours.HasValue);
    }
}
