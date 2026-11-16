using FluentValidation;

namespace TeamHub.Application.Notifications.Commands.RegisterDeviceToken;

public sealed class RegisterDeviceTokenValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenValidator()
    {
        // Expo push token (điện thoại) hoặc FCM registration token (web); độ dài khớp cột Token
        RuleFor(x => x.Token).NotEmpty().MaximumLength(PushTokenFormat.MaxLength)
            .Must(token => PushTokenFormat.IsExpo(token) || PushTokenFormat.IsFcm(token))
            .WithMessage("Token không phải Expo push token hoặc FCM registration token.");
        RuleFor(x => x.Platform).IsInEnum();
    }
}
