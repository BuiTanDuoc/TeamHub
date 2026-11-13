using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Users.Commands.SetUserSystemAdmin;

/// <summary>Cấp/thu hồi quyền SystemAdmin. Quyền trong token hiện có chỉ đổi khi họ đăng nhập lại hoặc làm mới token. Chỉ SystemAdmin.</summary>
public sealed record SetUserSystemAdminCommand(Guid UserId, bool IsSystemAdmin) : ICommand<Unit>, IRequireSystemAdmin;
