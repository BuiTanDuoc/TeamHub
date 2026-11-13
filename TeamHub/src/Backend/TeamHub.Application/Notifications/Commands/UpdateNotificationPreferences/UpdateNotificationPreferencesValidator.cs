using FluentValidation;
using TeamHub.Domain.Notifications;

namespace TeamHub.Application.Notifications.Commands.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
{
    public UpdateNotificationPreferencesValidator()
    {
        RuleFor(x => x.Items).NotEmpty()
            .Must(items => items.Count <= Enum.GetValues<NotificationType>().Length)
            .WithMessage("Danh sách vượt quá số loại thông báo.")
            .Must(items => items.Select(i => i.Type).Distinct().Count() == items.Count)
            .WithMessage("Mỗi loại thông báo chỉ được xuất hiện một lần.");

        RuleForEach(x => x.Items).ChildRules(item => item.RuleFor(i => i.Type).IsInEnum());
    }
}
