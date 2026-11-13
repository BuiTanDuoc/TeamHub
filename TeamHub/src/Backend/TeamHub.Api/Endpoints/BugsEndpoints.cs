using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Bugs.Commands.AssignBug;
using TeamHub.Application.Bugs.Commands.ChangeBugStatus;
using TeamHub.Application.Bugs.Commands.CreateBug;
using TeamHub.Application.Bugs.Commands.DeleteBug;
using TeamHub.Application.Bugs.Commands.MarkBugDuplicate;
using TeamHub.Application.Bugs.Commands.UpdateBug;
using TeamHub.Application.Bugs.Queries.GetBug;
using TeamHub.Application.Bugs.Queries.GetBugHistory;
using TeamHub.Application.Bugs.Queries.GetBugs;
using TeamHub.Application.Common;
using TeamHub.Domain.Bugs;

namespace TeamHub.Api.Endpoints;

public static class BugsEndpoints
{
    public static IEndpointRouteBuilder MapBugsEndpoints(this IEndpointRouteBuilder app)
    {
        // Bug luôn nằm dưới team/dự án để AuthorizationBehavior kiểm tra quyền theo TeamId
        var bugs = app.MapGroup("/api/v1/teams/{teamId:guid}/projects/{projectId:guid}/bugs")
            .WithTags("Bugs")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        bugs.MapGet("/", async (Guid teamId, Guid projectId, BugStatus? status, BugSeverity? severity, Guid? assigneeId,
                    string? search, int? cursor, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetBugsQuery(
                    teamId, projectId, status, severity, assigneeId, search, cursor,
                    pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách bug, mới nhất trước; phân trang bằng cursor (nextCursor → ?cursor=)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        bugs.MapPost("/", async (Guid teamId, Guid projectId, CreateBugRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var created = await mediator.Send(new CreateBugCommand(
                    teamId, projectId, body.Title, body.Severity, body.Priority, body.Environment, body.AffectedVersion,
                    body.StepsToReproduce, body.ExpectedResult, body.ActualResult, body.DeviceInfo), ct);
                return TypedResults.Created($"/api/v1/teams/{teamId}/projects/{projectId}/bugs/{created.Id}", created);
            })
            .WithSummary("Báo bug mới (Tester trở lên); trạng thái ban đầu New, hạn SLA tính theo severity")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        bugs.MapGet("/{bugId:guid}", async (Guid teamId, Guid projectId, Guid bugId, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetBugQuery(teamId, projectId, bugId), ct)))
            .WithSummary("Chi tiết bug kèm các trạng thái người xem được phép chuyển sang")
            .ProducesProblem(StatusCodes.Status404NotFound);

        bugs.MapPut("/{bugId:guid}", async (Guid teamId, Guid projectId, Guid bugId, UpdateBugRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UpdateBugCommand(
                    teamId, projectId, bugId, body.Title, body.Severity, body.Priority, body.Environment,
                    body.AffectedVersion, body.StepsToReproduce, body.ExpectedResult, body.ActualResult, body.DeviceInfo), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Sửa nội dung bug (Tester trở lên)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        bugs.MapPut("/{bugId:guid}/status", async (Guid teamId, Guid projectId, Guid bugId, ChangeBugStatusRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ChangeBugStatusCommand(teamId, projectId, bugId, body.Status, body.Note, body.FixedInVersion), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đổi trạng thái bug: sai chuyển → 409, sai vai trò → 403, thiếu lý do → 400")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        bugs.MapPut("/{bugId:guid}/assignee", async (Guid teamId, Guid projectId, Guid bugId, AssignBugRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new AssignBugCommand(teamId, projectId, bugId, body.AssigneeId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Giao bug cho thành viên dự án (Developer/TeamLead); Confirmed/Reopened tự chuyển sang Assigned")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        bugs.MapPut("/{bugId:guid}/duplicate", async (Guid teamId, Guid projectId, Guid bugId, MarkBugDuplicateRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new MarkBugDuplicateCommand(teamId, projectId, bugId, body.OriginalBugId, body.Note), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đánh dấu trùng với bug khác (chỉ TeamLead), lý do bắt buộc")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        bugs.MapDelete("/{bugId:guid}", async (Guid teamId, Guid projectId, Guid bugId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteBugCommand(teamId, projectId, bugId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Xóa mềm bug (TeamLead)")
            .ProducesProblem(StatusCodes.Status404NotFound);

        bugs.MapGet("/{bugId:guid}/history", async (Guid teamId, Guid projectId, Guid bugId, int? page, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetBugHistoryQuery(
                    teamId, projectId, bugId, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Lịch sử chuyển trạng thái của bug (ai, lúc nào, ghi chú)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
