using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Users.Queries.SearchUsers;

public sealed class SearchUsersHandler(IAppDbContext db) : IQueryHandler<SearchUsersQuery, PagedResult<UserSummaryDto>>
{
    public async ValueTask<PagedResult<UserSummaryDto>> Handle(SearchUsersQuery query, CancellationToken cancellationToken)
    {
        var users = db.Users.AsNoTracking();

        if (query.IsActive is { } isActive)
            users = users.Where(u => u.IsActive == isActive);

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            users = users.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));

        return await users
            .OrderBy(u => u.FullName).ThenBy(u => u.Id)
            .Select(u => new UserSummaryDto(u.Id, u.Email ?? string.Empty, u.FullName, u.AvatarUrl, u.IsActive, u.IsSystemAdmin))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);
    }
}
