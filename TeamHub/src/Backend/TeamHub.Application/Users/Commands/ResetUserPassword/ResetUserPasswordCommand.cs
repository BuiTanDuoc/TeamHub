using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Users.Commands.ResetUserPassword;

/// <summary>Đặt lại mật khẩu cho người dùng (quên mật khẩu) và thu hồi mọi phiên của họ. Chỉ SystemAdmin.</summary>
public sealed record ResetUserPasswordCommand(Guid UserId, string NewPassword) : ICommand<Unit>, IRequireSystemAdmin;
