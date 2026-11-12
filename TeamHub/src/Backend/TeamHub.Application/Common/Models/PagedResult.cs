using Microsoft.EntityFrameworkCore;

namespace TeamHub.Application.Common.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

/// <summary>Đầu vào phân trang chung (page bắt đầu từ 1).</summary>
public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
}

public static class PagedResultExtensions
{
    /// <summary>Query phải đã có ORDER BY ổn định (thêm khóa phụ) để các trang không bị trùng/sót.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total, page, pageSize);
    }
}
