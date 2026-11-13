namespace TeamHub.Application.Tasks;

/// <summary>Cấu hình Kanban board, mục "Kanban" trong appsettings.json.</summary>
public sealed class KanbanOptions
{
    public const string SectionName = "Kanban";

    /// <summary>Số thẻ tối đa trả về cho mỗi cột (cột Done có thể rất dài).</summary>
    public int MaxCardsPerColumn { get; set; } = 50;
}
