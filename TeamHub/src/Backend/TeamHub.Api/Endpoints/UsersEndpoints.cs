using Mediator;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Users.Queries.SearchUsers;

namespace TeamHub.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/", async (string? search, int? page, int? pageSize, IOptions<PagingOptions> paging, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new SearchUsersQuery(search, page ?? 1, pageSize ?? paging.Value.DefaultPageSize), ct)))
            .WithSummary("Tìm user theo tên/email (chỉ SystemAdmin)")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
