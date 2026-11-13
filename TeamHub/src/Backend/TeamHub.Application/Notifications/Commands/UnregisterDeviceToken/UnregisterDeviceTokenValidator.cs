using FluentValidation;

namespace TeamHub.Application.Notifications.Commands.UnregisterDeviceToken;

public sealed class UnregisterDeviceTokenValidator : AbstractValidator<UnregisterDeviceTokenCommand>
{
    public UnregisterDeviceTokenValidator() => RuleFor(x => x.Token).NotEmpty().MaximumLength(200);
}
