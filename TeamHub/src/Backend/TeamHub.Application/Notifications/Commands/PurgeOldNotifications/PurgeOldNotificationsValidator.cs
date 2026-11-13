using FluentValidation;

namespace TeamHub.Application.Notifications.Commands.PurgeOldNotifications;

public sealed class PurgeOldNotificationsValidator : AbstractValidator<PurgeOldNotificationsCommand>
{
    public PurgeOldNotificationsValidator()
    {
        // Danh sách Id đưa vào câu DELETE nên giới hạn kích thước lô
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 2000);
        RuleFor(x => x.AnyBefore).LessThanOrEqualTo(x => x.ReadBefore)
            .WithMessage("Mốc xóa mọi thông báo phải cũ hơn hoặc bằng mốc xóa thông báo đã đọc.");
    }
}
