using Mediator;

namespace TeamHub.Application.Notifications.Commands.UnregisterDeviceToken;

/// <summary>Gỡ token của thiết bị khỏi tài khoản (gọi khi đăng xuất để máy không nhận push của tài khoản cũ).</summary>
public sealed record UnregisterDeviceTokenCommand(string Token) : ICommand<Unit>;
