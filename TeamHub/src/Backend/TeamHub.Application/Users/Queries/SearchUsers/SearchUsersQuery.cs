using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Users.Queries.SearchUsers;

/// <summary>
/// Danh sách/tìm người dùng theo tên hoặc email. IsActive null = cả đang hoạt động và đã vô hiệu hóa
/// (chọn TeamLead hay thêm vào team nên truyền true). Chỉ SystemAdmin.
/// </summary>
public sealed record SearchUsersQuery(string? Search, bool? IsActive, int Page, int PageSize)
    : IQuery<PagedResult<UserSummaryDto>>, IPagedQuery, IRequireSystemAdmin;
