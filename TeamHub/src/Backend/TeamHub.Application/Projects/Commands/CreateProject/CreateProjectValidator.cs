using FluentValidation;

namespace TeamHub.Application.Projects.Commands.CreateProject;

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        // 2-10 ký tự, bắt đầu bằng chữ (khớp độ dài cột Key)
        RuleFor(x => x.Key).NotEmpty()
            .Matches("^[A-Za-z][A-Za-z0-9]{1,9}$").WithMessage("Key gồm 2-10 ký tự chữ/số và bắt đầu bằng chữ.");
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
    }
}
