using Mediator;
using TeamHub.Application.Users.Commands.ChangeMyPassword;
using TeamHub.Application.Users.Commands.UpdateMyProfile;

namespace TeamHub.Api.Endpoints;

public sealed record UpdateProfileRequest(string FullName, string? AvatarUrl);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        // Thao tác trên tài khoản của chính mình: mọi người dùng đã đăng nhập
        var group = app.MapGroup("/api/v1/account")
            .WithTags("Account")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/profile", async (UpdateProfileRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UpdateMyProfileCommand(body.FullName, body.AvatarUrl), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Sửa họ tên và ảnh đại diện của tôi (avatarUrl là https hoặc null để xóa)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/change-password", async (ChangePasswordRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ChangeMyPasswordCommand(body.CurrentPassword, body.NewPassword), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đổi mật khẩu của tôi; mọi phiên bị thu hồi nên phải đăng nhập lại")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
