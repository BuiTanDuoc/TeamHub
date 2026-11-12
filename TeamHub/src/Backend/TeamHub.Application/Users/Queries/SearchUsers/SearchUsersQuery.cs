using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Users.Queries.SearchUsers;

/// <summary>Tìm user đang hoạt động theo tên/email (để chọn TeamLead khi tạo team). Chỉ SystemAdmin.</summary>
public sealed record SearchUsersQuery(string? Search, int Page, int PageSize)
    : IQuery<PagedResult<UserSummaryDto>>, IPagedQuery, IRequireSystemAdmin;
