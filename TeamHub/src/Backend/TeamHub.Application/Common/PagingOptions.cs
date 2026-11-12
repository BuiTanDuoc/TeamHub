namespace TeamHub.Application.Common;

/// <summary>Giới hạn phân trang, cấu hình ở appsettings.json mục "Paging".</summary>
public sealed class PagingOptions
{
    public const string SectionName = "Paging";

    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
}
