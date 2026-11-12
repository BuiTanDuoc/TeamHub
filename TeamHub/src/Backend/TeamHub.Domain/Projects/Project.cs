using TeamHub.Domain.Common;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Domain.Projects;

public sealed class Project : AuditableEntity, ITeamScoped, ISoftDeletable
{
    private readonly List<ProjectMember> _members = [];

    private Project() { } // EF Core

    public Project(Guid teamId, string name, string key, string? description,
        DateOnly? startDate, DateOnly? endDate, bool useSprints)
    {
        EnsureDates(startDate, endDate);

        TeamId = teamId;
        Name = name.Trim();
        Key = key.Trim().ToUpperInvariant();
        Description = description?.Trim();
        StartDate = startDate;
        EndDate = endDate;
        UseSprints = useSprints;
    }

    public Guid TeamId { get; private set; }
    public string Name { get; private set; } = null!;

    /// <summary>Mã ngắn của dự án, dùng làm tiền tố (vd: TH-123). Duy nhất trong team.</summary>
    public string Key { get; private set; } = null!;

    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Planning;
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    /// <summary>Bật Sprint cho dự án; mặc định dự án chạy Kanban.</summary>
    public bool UseSprints { get; private set; }

    /// <summary>Bộ đếm số thứ tự dùng chung cho WorkItem và Bug trong dự án.</summary>
    public int NextItemNumber { get; private set; } = 1;

    public byte[] RowVersion { get; private set; } = [];

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Team Team { get; private set; } = null!;
    public IReadOnlyCollection<ProjectMember> Members => _members;

    public void Update(string name, string? description, DateOnly? startDate, DateOnly? endDate, bool useSprints)
    {
        EnsureDates(startDate, endDate);
        Name = name.Trim();
        Description = description?.Trim();
        StartDate = startDate;
        EndDate = endDate;
        UseSprints = useSprints;
    }

    public void ChangeStatus(ProjectStatus status) => Status = status;

    /// <summary>
    /// Cấp số thứ tự cho WorkItem/Bug mới. RowVersion của Project bảo vệ khỏi cấp trùng số
    /// khi hai request chạy song song: request sau sẽ gặp DbUpdateConcurrencyException và cần thử lại.
    /// </summary>
    public int ReserveItemNumber()
    {
        var number = NextItemNumber;
        NextItemNumber++;
        return number;
    }

    // Lưu ý: yêu cầu handler đã .Include(p => p.Members)
    public ProjectMember AddMember(Guid userId, DateTimeOffset addedAt)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new DomainException("Người dùng đã tham gia dự án.");

        var member = new ProjectMember(TeamId, Id, userId, addedAt);
        _members.Add(member);
        return member;
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
                     ?? throw new DomainException("Người dùng không thuộc dự án.");
        _members.Remove(member);
    }

    private static void EnsureDates(DateOnly? start, DateOnly? end)
    {
        if (start is not null && end is not null && end < start)
            throw new DomainException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
    }
}
