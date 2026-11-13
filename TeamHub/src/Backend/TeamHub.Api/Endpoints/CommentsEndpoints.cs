using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Comments.Commands.AddComment;
using TeamHub.Application.Comments.Commands.DeleteComment;
using TeamHub.Application.Comments.Commands.EditComment;
using TeamHub.Application.Comments.Queries.GetComments;
using TeamHub.Application.Common;

namespace TeamHub.Api.Endpoints;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder MapCommentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/teams/{teamId:guid}/projects/{projectId:guid}/tasks/{taskId:guid}/comments")
            .WithTags("Comments")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (Guid teamId, Guid projectId, Guid taskId, int? page, int? pageSize,
                    IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetCommentsQuery(
                    teamId, projectId, taskId, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Bình luận của task, cũ nhất trước (mọi thành viên team)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (Guid teamId, Guid projectId, Guid taskId, CommentRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(new AddCommentCommand(teamId, projectId, taskId, body.Body), ct);
                return TypedResults.Created($"/api/v1/teams/{teamId}/projects/{projectId}/tasks/{taskId}/comments", new IdResponse(id));
            })
            .WithSummary("Thêm bình luận (Tester trở lên)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{commentId:guid}", async (Guid teamId, Guid projectId, Guid taskId, Guid commentId, CommentRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new EditCommentCommand(teamId, projectId, taskId, commentId, body.Body), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Sửa bình luận (chỉ tác giả)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{commentId:guid}", async (Guid teamId, Guid projectId, Guid taskId, Guid commentId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteCommentCommand(teamId, projectId, taskId, commentId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Xóa mềm bình luận (tác giả, TeamLead hoặc SystemAdmin)")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
