using FluentValidation;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(20000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.EstimateHours).InclusiveBetween(0m, 99999m).When(x => x.EstimateHours.HasValue);

        RuleFor(x => x.ParentId).NotNull()
            .When(x => x.Type == WorkItemType.SubTask)
            .WithMessage("Sub-task bắt buộc phải có task cha.");
        RuleFor(x => x.ParentId).Null()
            .When(x => x.Type == WorkItemType.Epic)
            .WithMessage("Epic không thể có item cha.");
    }
}
