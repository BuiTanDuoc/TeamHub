using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Notifications.Commands.MarkAllNotificationsRead;
using TeamHub.Application.Notifications.Commands.MarkNotificationRead;
using TeamHub.Application.Notifications.Commands.UpdateNotificationPreferences;
using TeamHub.Application.Notifications.Queries.GetMyNotifications;
using TeamHub.Application.Notifications.Queries.GetNotificationPreferences;
using TeamHub.Application.Notifications.Queries.GetUnreadNotificationCount;

namespace TeamHub.Api.Endpoints;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/", async (bool? unreadOnly, string? cursor, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetMyNotificationsQuery(
                    unreadOnly ?? false,
                    string.IsNullOrEmpty(cursor) ? null : cursor,
                    pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Thông báo của tôi, mới nhất trước; phân trang bằng cursor (nextCursor → ?cursor=), unreadOnly=true để chỉ lấy chưa đọc")
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

        group.MapGet("/preferences", async (IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetNotificationPreferencesQuery(), ct)))
            .WithSummary("Cài đặt nhận thông báo của tôi cho từng loại (mặc định bật cả trong app và push)");

        group.MapPut("/preferences", async (UpdateNotificationPreferencesRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UpdateNotificationPreferencesCommand(body.Items), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Lưu cài đặt cho các loại thông báo gửi lên; loại không gửi giữ nguyên")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}

public sealed record UpdateNotificationPreferencesRequest(IReadOnlyList<NotificationPreferenceItem> Items);
