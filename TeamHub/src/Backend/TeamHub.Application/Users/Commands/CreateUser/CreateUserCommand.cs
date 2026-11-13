using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Users.Commands.CreateUser;

/// <summary>SystemAdmin tạo tài khoản với mật khẩu ban đầu. Trả về Id người dùng; sau đó thêm vào team bằng email.</summary>
public sealed record CreateUserCommand(string Email, string FullName, string Password, bool IsSystemAdmin)
    : ICommand<Guid>, IRequireSystemAdmin;
