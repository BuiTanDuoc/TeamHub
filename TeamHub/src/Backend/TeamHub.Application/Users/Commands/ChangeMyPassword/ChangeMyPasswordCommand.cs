using Mediator;

namespace TeamHub.Application.Users.Commands.ChangeMyPassword;

/// <summary>Tự đổi mật khẩu; mọi phiên bị thu hồi nên client phải đăng nhập lại.</summary>
public sealed record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword) : ICommand<Unit>;
