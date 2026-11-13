using Mediator;
using TeamHub.Application.Notifications.Commands.RegisterDeviceToken;
using TeamHub.Application.Notifications.Commands.UnregisterDeviceToken;
using TeamHub.Domain.Notifications;

namespace TeamHub.Api.Endpoints;

public sealed record RegisterDeviceRequest(string Token, DevicePlatform Platform);

public static class DevicesEndpoints
{
    public static IEndpointRouteBuilder MapDevicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/devices")
            .WithTags("Devices")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/", async (RegisterDeviceRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new RegisterDeviceTokenCommand(body.Token, body.Platform), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đăng ký Expo push token của thiết bị cho tài khoản hiện tại (gọi lại mỗi lần mở app)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/", async (string token, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UnregisterDeviceTokenCommand(token), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Gỡ token của thiết bị khỏi tài khoản (gọi khi đăng xuất); token truyền qua ?token=")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
