using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Users.Commands.SetUserActive;

/// <summary>
/// Vô hiệu hóa/kích hoạt lại tài khoản. Vô hiệu hóa sẽ thu hồi mọi phiên và gỡ thiết bị nhận push;
/// dữ liệu và vai trò trong team được giữ nguyên để kích hoạt lại không mất gì. Chỉ SystemAdmin.
/// </summary>
public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : ICommand<Unit>, IRequireSystemAdmin;
