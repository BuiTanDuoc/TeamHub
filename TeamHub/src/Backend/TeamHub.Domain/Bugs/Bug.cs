using TeamHub.Domain.Common;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Projects;
using TeamHub.Domain.Teams;
using TeamHub.Domain.Users;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Domain.Bugs;

public sealed class Bug : AuditableEntity, ITeamScoped, ISoftDeletable
{
    private readonly List<BugStatusHistory> _history = [];

    private Bug() { } // EF Core

    public Bug(Guid teamId, Guid projectId, int number, string title, BugSeverity severity,
        Priority priority, BugEnvironment environment, string stepsToReproduce,
        string expectedResult, string actualResult, Guid reporterId, DateTimeOffset now)
    {
        TeamId = teamId;
        ProjectId = projectId;
        Number = number;
        Title = title.Trim();
        Severity = severity;
        Priority = priority;
        Environment = environment;
        StepsToReproduce = stepsToReproduce.Trim();
        ExpectedResult = expectedResult.Trim();
        ActualResult = actualResult.Trim();
        ReporterId = reporterId;

        _history.Add(new BugStatusHistory(teamId, Id, null, BugStatus.New, reporterId, now, null));
    }

    public Guid TeamId { get; private set; }
    public Guid ProjectId { get; private set; }

    /// <summary>Số thứ tự trong dự án (dùng chung bộ đếm với WorkItem). Mã hiển thị: {Project.Key}-{Number}.</summary>
    public int Number { get; private set; }

    public string Title { get; private set; } = null!;
    public BugSeverity Severity { get; private set; }
    public Priority Priority { get; private set; }
    public BugStatus Status { get; private set; } = BugStatus.New;
    public BugEnvironment Environment { get; private set; }

    /// <summary>Phiên bản phát hiện lỗi.</summary>
    public string? AffectedVersion { get; private set; }

    /// <summary>Phiên bản đã sửa lỗi.</summary>
    public string? FixedInVersion { get; private set; }

    public string StepsToReproduce { get; private set; } = null!;
    public string ExpectedResult { get; private set; } = null!;
    public string ActualResult { get; private set; } = null!;

    /// <summary>Thiết bị/OS/trình duyệt (đặc biệt hữu ích với bug trên mobile).</summary>
    public string? DeviceInfo { get; private set; }

    public Guid ReporterId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid? LinkedWorkItemId { get; private set; }
    public Guid? DuplicateOfBugId { get; private set; }

    /// <summary>Hạn xử lý theo SLA, do Application tính từ cấu hình appsettings theo Severity.</summary>
    public DateTimeOffset? SlaDueAt { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public int ReopenCount { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Project Project { get; private set; } = null!;
    public AppUser Reporter { get; private set; } = null!;
    public AppUser? Assignee { get; private set; }
    public WorkItem? LinkedWorkItem { get; private set; }
    public Bug? DuplicateOf { get; private set; }
    public IReadOnlyCollection<BugStatusHistory> History => _history;

    public void Update(string title, BugSeverity severity, Priority priority, BugEnvironment environment,
        string? affectedVersion, string stepsToReproduce, string expectedResult, string actualResult,
        string? deviceInfo)
    {
        Title = title.Trim();
        Severity = severity;
        Priority = priority;
        Environment = environment;
        AffectedVersion = affectedVersion?.Trim();
        StepsToReproduce = stepsToReproduce.Trim();
        ExpectedResult = expectedResult.Trim();
        ActualResult = actualResult.Trim();
        DeviceInfo = deviceInfo?.Trim();
    }

    public void LinkWorkItem(Guid? workItemId) => LinkedWorkItemId = workItemId;

    public void SetSlaDueAt(DateTimeOffset? dueAt) => SlaDueAt = dueAt;

    public void SetFixedInVersion(string? version) => FixedInVersion = version?.Trim();

    /// <summary>
    /// Giao bug cho người xử lý. Nếu bug đang Confirmed/Reopened thì tự chuyển sang Assigned;
    /// nếu đang Assigned/InProgress thì chỉ đổi người xử lý (giao lại).
    /// </summary>
    public void Assign(Guid assigneeId, Guid actorId, TeamRole actorRole, DateTimeOffset now)
    {
        if (Status is BugStatus.Confirmed or BugStatus.Reopened)
        {
            ChangeStatus(BugStatus.Assigned, actorId, actorRole, now);
        }
        else if (Status is BugStatus.Assigned or BugStatus.InProgress)
        {
            if (!BugWorkflow.IsRoleAllowed(BugStatus.Assigned, actorRole))
                throw new ForbiddenTransitionException($"Vai trò {actorRole} không được phép giao lại bug.");
        }
        else
        {
            throw new InvalidTransitionException("Bug", Status.ToString(), BugStatus.Assigned.ToString());
        }

        AssigneeId = assigneeId;
    }

    public void MarkDuplicate(Guid originalBugId, Guid actorId, TeamRole actorRole, DateTimeOffset now, string? note)
    {
        if (originalBugId == Id)
            throw new DomainException("Bug không thể trùng với chính nó.");

        ChangeStatus(BugStatus.Duplicate, actorId, actorRole, now, note);
        DuplicateOfBugId = originalBugId;
    }

    /// <summary>
    /// Chuyển trạng thái theo workflow: kiểm tra chuyển hợp lệ, vai trò, lý do bắt buộc,
    /// cập nhật mốc thời gian và ghi lịch sử.
    /// </summary>
    public void ChangeStatus(BugStatus to, Guid actorId, TeamRole actorRole, DateTimeOffset now, string? note = null)
    {
        BugWorkflow.EnsureCanTransition(Status, to);

        if (!BugWorkflow.IsRoleAllowed(to, actorRole))
            throw new ForbiddenTransitionException($"Vai trò {actorRole} không được phép chuyển bug sang '{to}'.");

        if (BugWorkflow.RequiresNote(to) && string.IsNullOrWhiteSpace(note))
            throw new DomainException($"Chuyển bug sang '{to}' bắt buộc phải có lý do.");

        var from = Status;
        Status = to;

        switch (to)
        {
            case BugStatus.Fixed:
                ResolvedAt = now;
                break;
            case BugStatus.Closed:
                ClosedAt = now;
                break;
            case BugStatus.Reopened:
                ReopenCount++;
                ResolvedAt = null;
                ClosedAt = null;
                break;
        }

        _history.Add(new BugStatusHistory(TeamId, Id, from, to, actorId, now, note?.Trim()));
    }
}
