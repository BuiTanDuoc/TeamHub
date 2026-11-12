namespace TeamHub.Domain.Common;

/// <summary>
/// Tên các named query filter (EF Core 10). Cho phép bỏ qua riêng từng filter:
/// <c>.IgnoreQueryFilters([QueryFilterNames.SoftDelete])</c> thay vì tắt toàn bộ.
/// </summary>
public static class QueryFilterNames
{
    public const string TeamScope = "TeamScope";
    public const string SoftDelete = "SoftDelete";
}
