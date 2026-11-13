using FluentValidation;

namespace TeamHub.Application.Users.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    // Chính sách độ mạnh mật khẩu do Identity kiểm tra (Identity:Password); ở đây chỉ chặn dữ liệu rỗng/quá dài
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
