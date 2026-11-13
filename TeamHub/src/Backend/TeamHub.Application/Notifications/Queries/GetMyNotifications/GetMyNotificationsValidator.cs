using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsValidator(IOptions<PagingOptions> options)
    : PagedQueryValidator<GetMyNotificationsQuery>(options);
