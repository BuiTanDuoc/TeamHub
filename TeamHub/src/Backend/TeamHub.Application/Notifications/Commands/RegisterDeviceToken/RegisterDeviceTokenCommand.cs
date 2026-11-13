using Mediator;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications.Commands.RegisterDeviceToken;

/// <summary>Đăng ký (hoặc làm mới) Expo push token của thiết bị cho người dùng hiện tại. Gọi lại mỗi lần mở app là an toàn.</summary>
public sealed record RegisterDeviceTokenCommand(string Token, DevicePlatform Platform) : ICommand<Unit>;
