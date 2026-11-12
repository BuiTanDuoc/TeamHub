using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Teams.Commands.AddTeamMember;
using TeamHub.Application.Teams.Commands.ChangeTeamMemberRole;
using TeamHub.Application.Teams.Commands.CreateTeam;
using TeamHub.Application.Teams.Commands.RemoveTeamMember;
using TeamHub.Application.Teams.Commands.SetTeamActive;
using TeamHub.Application.Teams.Commands.UpdateTeam;
using TeamHub.Application.Teams.Queries.GetMyTeams;
using TeamHub.Application.Teams.Queries.GetTeam;
using TeamHub.Application.Teams.Queries.GetTeamMembers;

namespace TeamHub.Api.Endpoints;

public static class TeamsEndpoints
{
    public static IEndpointRouteBuilder MapTeamsEndpoints(this IEndpointRouteBuilder app)
    {
        // Yêu cầu đăng nhập nhờ fallback policy; quyền theo team do AuthorizationBehavior kiểm tra
        var group = app.MapGroup("/api/v1/teams")
            .WithTags("Teams")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/", async (int? page, int? pageSize, IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new GetMyTeamsQuery(page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách team của tôi (SystemAdmin thấy tất cả)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (CreateTeamRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(new CreateTeamCommand(body.Name, body.Code, body.Description, body.LeadUserId), ct);
                return TypedResults.Created($"/api/v1/teams/{id}", new IdResponse(id));
            })
            .WithSummary("Tạo team và chỉ định TeamLead đầu tiên (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{teamId:guid}", async (Guid teamId, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetTeamQuery(teamId), ct)))
            .WithSummary("Chi tiết team (mọi thành viên)")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{teamId:guid}", async (Guid teamId, UpdateTeamRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UpdateTeamCommand(teamId, body.Name, body.Description), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Cập nhật tên/mô tả team (TeamLead)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{teamId:guid}/active", async (Guid teamId, SetTeamActiveRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new SetTeamActiveCommand(teamId, body.IsActive), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Ngừng/kích hoạt lại team (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        var members = group.MapGroup("/{teamId:guid}/members");

        members.MapGet("/", async (Guid teamId, int? page, int? pageSize, IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new GetTeamMembersQuery(teamId, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách thành viên của team (mọi thành viên)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        members.MapPost("/", async (Guid teamId, AddTeamMemberRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var userId = await mediator.Send(new AddTeamMemberCommand(teamId, body.Email, body.Role, body.WeeklyCapacityHours), ct);
                return TypedResults.Created($"/api/v1/teams/{teamId}/members", new IdResponse(userId));
            })
            .WithSummary("Thêm thành viên theo email (TeamLead); user phải đã có tài khoản")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        members.MapPut("/{userId:guid}/role", async (Guid teamId, Guid userId, ChangeTeamMemberRoleRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ChangeTeamMemberRoleCommand(teamId, userId, body.Role), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đổi vai trò thành viên (TeamLead); không được hạ TeamLead cuối cùng")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        members.MapDelete("/{userId:guid}", async (Guid teamId, Guid userId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new RemoveTeamMemberCommand(teamId, userId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Xóa thành viên khỏi team (TeamLead); không được xóa TeamLead cuối cùng")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
