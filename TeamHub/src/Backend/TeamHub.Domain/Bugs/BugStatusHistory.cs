using TeamHub.Domain.Common;
using TeamHub.Domain.Users;

namespace TeamHub.Domain.Bugs;

public sealed class BugStatusHistory : BaseEntity, ITeamScoped
{
    private BugStatusHistory() { } // EF Core

    internal BugStatusHistory(Guid teamId, Guid bugId, BugStatus? from, BugStatus to,
        Guid changedById, DateTimeOffset changedAt, string? note)
    {
        TeamId = teamId;
        BugId = bugId;
        FromStatus = from;
        ToStatus = to;
        ChangedById = changedById;
        ChangedAt = changedAt;
        Note = note;
    }

    /// <summary>Phi chuẩn hóa từ Bug để áp global query filter theo team.</summary>
    public Guid TeamId { get; private set; }

    public Guid BugId { get; private set; }

    /// <summary>Null ở bản ghi đầu tiên (lúc tạo bug).</summary>
    public BugStatus? FromStatus { get; private set; }

    public BugStatus ToStatus { get; private set; }
    public Guid ChangedById { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string? Note { get; private set; }

    public AppUser ChangedBy { get; private set; } = null!;
}
