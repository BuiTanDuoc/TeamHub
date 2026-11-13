using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Projects.Commands.AddProjectMember;
using TeamHub.Application.Projects.Commands.ChangeProjectStatus;
using TeamHub.Application.Projects.Commands.CreateProject;
using TeamHub.Application.Projects.Commands.DeleteProject;
using TeamHub.Application.Projects.Commands.RemoveProjectMember;
using TeamHub.Application.Projects.Commands.UpdateProject;
using TeamHub.Application.Projects.Queries.GetProject;
using TeamHub.Application.Projects.Queries.GetProjectMembers;
using TeamHub.Application.Projects.Queries.GetProjects;
using TeamHub.Domain.Projects;

namespace TeamHub.Api.Endpoints;

public static class ProjectsEndpoints
{
    public static IEndpointRouteBuilder MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        // Dự án luôn nằm dưới team để AuthorizationBehavior kiểm tra quyền theo TeamId
        var group = app.MapGroup("/api/v1/teams/{teamId:guid}/projects")
            .WithTags("Projects")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/", async (Guid teamId, ProjectStatus? status, int? page, int? pageSize, IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new GetProjectsQuery(teamId, status, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách dự án của team (mọi thành viên), lọc theo trạng thái")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (Guid teamId, CreateProjectRequest body, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(new CreateProjectCommand(
                    teamId, body.Name, body.Key, body.Description, body.StartDate, body.EndDate, body.UseSprints), ct);
                return TypedResults.Created($"/api/v1/teams/{teamId}/projects/{id}", new IdResponse(id));
            })
            .WithSummary("Tạo dự án (TeamLead); Key duy nhất trong team")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{projectId:guid}", async (Guid teamId, Guid projectId, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(new GetProjectQuery(teamId, projectId), ct)))
            .WithSummary("Chi tiết dự án (mọi thành viên team)")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{projectId:guid}", async (Guid teamId, Guid projectId, UpdateProjectRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new UpdateProjectCommand(
                    teamId, projectId, body.Name, body.Description, body.StartDate, body.EndDate, body.UseSprints), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Cập nhật thông tin dự án (TeamLead); không đổi được Key")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{projectId:guid}/status", async (Guid teamId, Guid projectId, ChangeProjectStatusRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ChangeProjectStatusCommand(teamId, projectId, body.Status), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Đổi trạng thái dự án (TeamLead)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{projectId:guid}", async (Guid teamId, Guid projectId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteProjectCommand(teamId, projectId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Xóa mềm dự án (TeamLead)")
            .ProducesProblem(StatusCodes.Status404NotFound);

        var members = group.MapGroup("/{projectId:guid}/members");

        members.MapGet("/", async (Guid teamId, Guid projectId, int? page, int? pageSize, IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new GetProjectMembersQuery(teamId, projectId, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Danh sách thành viên dự án (mọi thành viên team)")
            .ProducesProblem(StatusCodes.Status400BadRequest);

        members.MapPost("/", async (Guid teamId, Guid projectId, AddProjectMemberRequest body, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new AddProjectMemberCommand(teamId, projectId, body.UserId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Thêm thành viên team vào dự án (TeamLead)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        members.MapDelete("/{userId:guid}", async (Guid teamId, Guid projectId, Guid userId, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new RemoveProjectMemberCommand(teamId, projectId, userId), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Xóa thành viên khỏi dự án (TeamLead)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
