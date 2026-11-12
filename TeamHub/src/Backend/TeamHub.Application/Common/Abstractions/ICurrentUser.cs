namespace TeamHub.Application.Common.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// True khi user là SystemAdmin (đọc từ JWT) nên được bỏ qua lọc theo team.
    /// Tiến trình nền (Hangfire job) KHÔNG có user: job phải gọi
    /// <c>.IgnoreQueryFilters([QueryFilterNames.TeamScope])</c> một cách tường minh, kèm comment lý do.
    /// </summary>
    bool IsSystemAdmin { get; }
}
