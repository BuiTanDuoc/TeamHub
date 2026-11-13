using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Tasks.Commands.AssignTask;
using TeamHub.Application.Tasks.Commands.ChangeTaskStatus;
using TeamHub.Application.Tasks.Commands.CreateTask;
using TeamHub.Application.Tasks.Commands.DeleteTask;
using TeamHub.Application.Tasks.Commands.UpdateTaskDetails;
using TeamHub.Application.Tasks.Queries.GetKanbanBoard;
using TeamHub.Application.Tasks.Queries.GetTask;
using TeamHub.Application.Tasks.Queries.GetTaskHistory;
using TeamHub.Application.Tasks.Queries.GetTasks;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Api.Endpoints;

public static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        // Task luôn nằm dưới team/dự án để AuthorizationBehavior kiểm tra quyền theo TeamId
        var project = app.MapGroup("/api/v1/teams/{teamId:guid}/projects/{projectId:guid}")
            .WithTags("Tasks")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        project.MapGet("/board", async (Guid teamId, Guid projectId, Guid? assigneeId, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetKanbanBoardQuery(teamId, projectId, assigneeId), ct)))
            .WithSummary("Kanban board: mỗi trạng thái một cột, giới hạn số thẻ mỗi cột (mọi thành viên team)")
            .ProducesProblem(StatusCodes.Status404NotFound);

        var tasks = project.MapGroup("/tasks");

        tasks.MapGet("/", async (Guid teamId, Guid projectId, WorkItemStatus? status, WorkItemType? type, Guid? assigneeId,
                    Guid? parentId, string? search, int? cursor, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetTasksQuery(
                    teamId, projectId, status, type, assigneeId, parentId, search, cursor,
                    pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách task, mới nhất trước; phân trang bằng cursor (nextCursor → ?cursor=)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        tasks.MapPost("/", async (Guid teamId, Guid projectId, CreateTaskRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var created = await mediator.Send(new CreateTaskCommand(
                    teamId, projectId, body.Type, body.Title, body.Description, body.Priority,
                    body.AssigneeId, body.ParentId, body.EstimateHours, body.DueDate), ct);
                return TypedResults.Created($"/api/v1/teams/{teamId}/projects/{projectId}/tasks/{created.Id}", created);
            })
            .WithSummary("Tạo task (Tester trở lên); mã hiển thị {Key}-{Number}")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tasks.MapGet("/{taskId:guid}", async (Guid teamId, Guid projectId, Guid taskId, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetTaskQuery(teamId, projectId, taskId), ct)))
            .WithSummary("Chi tiết task kèm các trạng thái được phép chuyển sang")
            .ProducesProblem(StatusCodes.Status404NotFound);

        tasks.MapPut("/{taskId:guid}", async (Guid teamId, Guid projectId, Guid taskId, UpdateTaskRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UpdateTaskDetailsCommand(
                    teamId, projectId, taskId, body.Title, body.Description, body.Priority, body.EstimateHours, body.DueDate), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Sửa nội dung task (Tester trở lên)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        tasks.MapPut("/{taskId:guid}/status", async (Guid teamId, Guid projectId, Guid taskId, ChangeTaskStatusRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ChangeTaskStatusCommand(teamId, projectId, taskId, body.Status), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đổi trạng thái (kéo thẻ sang cột khác); chuyển không hợp lệ trả 409")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        tasks.MapPut("/{taskId:guid}/assignee", async (Guid teamId, Guid projectId, Guid taskId, AssignTaskRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new AssignTaskCommand(teamId, projectId, taskId, body.AssigneeId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Giao task cho thành viên dự án; assigneeId null để bỏ giao")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        tasks.MapDelete("/{taskId:guid}", async (Guid teamId, Guid projectId, Guid taskId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteTaskCommand(teamId, projectId, taskId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Xóa mềm task (TeamLead); không xóa được khi còn item con")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tasks.MapGet("/{taskId:guid}/history", async (Guid teamId, Guid projectId, Guid taskId, int? page, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetTaskHistoryQuery(
                    teamId, projectId, taskId, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Lịch sử đổi trạng thái/assignee/estimate của task")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
