using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Notifications.Commands.MarkAllNotificationsRead;
using TeamHub.Application.Notifications.Commands.MarkNotificationRead;
using TeamHub.Application.Notifications.Queries.GetMyNotifications;
using TeamHub.Application.Notifications.Queries.GetUnreadNotificationCount;

namespace TeamHub.Api.Endpoints;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/", async (bool? unreadOnly, int? page, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetMyNotificationsQuery(
                    unreadOnly ?? false, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Thông báo của tôi, mới nhất trước; unreadOnly=true để chỉ lấy chưa đọc")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/unread-count", async (IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetUnreadNotificationCountQuery(), ct)))
            .WithSummary("Số thông báo chưa đọc (hiển thị trên chuông)");

        group.MapPut("/{notificationId:guid}/read", async (Guid notificationId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new MarkNotificationReadCommand(notificationId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đánh dấu một thông báo đã đọc")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/read-all", async (IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(new { updated = await mediator.Send(new MarkAllNotificationsReadCommand(), ct) }))
            .WithSummary("Đánh dấu tất cả thông báo của tôi đã đọc");

        return app;
    }
}
