using FluentValidation;

namespace TeamHub.Application.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);

        // Chỉ nhận đường dẫn https tuyệt đối để client không bị trỏ tới nguồn không an toàn
        RuleFor(x => x.AvatarUrl).MaximumLength(500)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl))
            .WithMessage("Ảnh đại diện phải là đường dẫn https hợp lệ.");
    }
}
