using FluentValidation;

namespace TeamHub.Application.Notifications.Commands.RegisterDeviceToken;

public sealed class RegisterDeviceTokenValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenValidator()
    {
        // Dạng ExponentPushToken[xxxx] hoặc ExpoPushToken[xxxx]; 200 khớp độ dài cột Token
        RuleFor(x => x.Token).NotEmpty().MaximumLength(200)
            .Matches(@"^Expo(nent)?PushToken\[.+\]$").WithMessage("Token không phải Expo push token.");
        RuleFor(x => x.Platform).IsInEnum();
    }
}
