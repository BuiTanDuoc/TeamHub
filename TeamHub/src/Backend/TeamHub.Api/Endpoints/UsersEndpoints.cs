using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Users.Commands.CreateUser;
using TeamHub.Application.Users.Commands.ResetUserPassword;
using TeamHub.Application.Users.Commands.SetUserActive;
using TeamHub.Application.Users.Commands.SetUserSystemAdmin;
using TeamHub.Application.Users.Queries.GetUser;
using TeamHub.Application.Users.Queries.SearchUsers;

namespace TeamHub.Api.Endpoints;

public sealed record CreateUserRequest(string Email, string FullName, string Password, bool IsSystemAdmin = false);

public sealed record SetUserActiveRequest(bool IsActive);

public sealed record SetUserSystemAdminRequest(bool IsSystemAdmin);

public sealed record ResetUserPasswordRequest(string NewPassword);

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        // Quản lý tài khoản toàn hệ thống: tất cả đều chỉ dành cho SystemAdmin (AuthorizationBehavior kiểm tra)
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (string? search, bool? isActive, int? page, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new SearchUsersQuery(search, isActive, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách/tìm người dùng theo tên hoặc email; isActive=true để chỉ lấy tài khoản đang hoạt động (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (CreateUserRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(new CreateUserCommand(body.Email, body.FullName, body.Password, body.IsSystemAdmin), ct);
                return TypedResults.Created($"/api/v1/users/{id}", new IdResponse(id));
            })
            .WithSummary("Tạo tài khoản với mật khẩu ban đầu (chỉ SystemAdmin); sau đó thêm vào team bằng email")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{userId:guid}", async (Guid userId, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetUserQuery(userId), ct)))
            .WithSummary("Chi tiết người dùng kèm các team và vai trò (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{userId:guid}/active", async (Guid userId, SetUserActiveRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new SetUserActiveCommand(userId, body.IsActive), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Vô hiệu hóa/kích hoạt tài khoản; vô hiệu hóa thu hồi mọi phiên và gỡ thiết bị push (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{userId:guid}/system-admin", async (Guid userId, SetUserSystemAdminRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new SetUserSystemAdminCommand(userId, body.IsSystemAdmin), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Cấp/thu hồi quyền SystemAdmin; luôn phải còn ít nhất một SystemAdmin hoạt động (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{userId:guid}/reset-password", async (Guid userId, ResetUserPasswordRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ResetUserPasswordCommand(userId, body.NewPassword), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đặt lại mật khẩu cho người dùng và thu hồi mọi phiên của họ (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
