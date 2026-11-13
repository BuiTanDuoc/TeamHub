using FluentValidation;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;

namespace TeamHub.Application.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsValidator : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsValidator(IOptions<PagingOptions> options)
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, options.Value.MaxPageSize);
        RuleFor(x => x.Cursor)
            .Must(cursor => NotificationCursor.TryDecode(cursor, out _, out _))
            .When(x => x.Cursor is not null)
            .WithMessage("Cursor không hợp lệ.");
    }
}
