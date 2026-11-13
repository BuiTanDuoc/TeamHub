using TeamHub.Domain.Bugs;

namespace TeamHub.Application.Bugs;

/// <summary>Cấu hình bug, mục "Bugs" trong appsettings.json.</summary>
public sealed class BugOptions
{
    public const string SectionName = "Bugs";

    /// <summary>Hạn xử lý (giờ) theo mức độ nghiêm trọng, ví dụ Critical: 24. Thiếu mức nào thì bug mức đó không có SLA.</summary>
    public Dictionary<BugSeverity, int> Sla { get; set; } = [];

    public TimeSpan? SlaFor(BugSeverity severity) =>
        Sla.TryGetValue(severity, out var hours) && hours > 0 ? TimeSpan.FromHours(hours) : null;
}
