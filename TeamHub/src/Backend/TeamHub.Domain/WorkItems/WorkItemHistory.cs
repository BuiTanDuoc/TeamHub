using TeamHub.Domain.Common;
using TeamHub.Domain.Users;

namespace TeamHub.Domain.WorkItems;

/// <summary>Lịch sử đổi trạng thái/assignee/estimate của task (xem CLAUDE.md mục 6.1).</summary>
public sealed class WorkItemHistory : BaseEntity, ITeamScoped
{
    private WorkItemHistory() { } // EF Core

    public WorkItemHistory(Guid teamId, Guid workItemId, WorkItemField field,
        string? oldValue, string? newValue, Guid changedById, DateTimeOffset changedAt)
    {
        TeamId = teamId;
        WorkItemId = workItemId;
        Field = field;
        OldValue = oldValue;
        NewValue = newValue;
        ChangedById = changedById;
        ChangedAt = changedAt;
    }

    /// <summary>Phi chuẩn hóa từ WorkItem để áp global query filter theo team.</summary>
    public Guid TeamId { get; private set; }

    public Guid WorkItemId { get; private set; }
    public WorkItemField Field { get; private set; }

    /// <summary>Giá trị hiển thị được (trạng thái, tên người, số giờ), không lưu Id.</summary>
    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }
    public Guid ChangedById { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }

    public AppUser ChangedBy { get; private set; } = null!;
}
