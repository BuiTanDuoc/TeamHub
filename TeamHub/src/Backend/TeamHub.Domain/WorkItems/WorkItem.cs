using TeamHub.Domain.Common;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Projects;
using TeamHub.Domain.Users;

namespace TeamHub.Domain.WorkItems;

public sealed class WorkItem : AuditableEntity, ITeamScoped, ISoftDeletable
{
    private WorkItem() { } // EF Core

    public WorkItem(Guid teamId, Guid projectId, int number, WorkItemType type,
        string title, Guid reporterId, Guid? parentId = null)
    {
        if (type == WorkItemType.SubTask && parentId is null)
            throw new DomainException("Sub-task bắt buộc phải có task cha.");
        if (type == WorkItemType.Epic && parentId is not null)
            throw new DomainException("Epic không thể có item cha.");

        TeamId = teamId;
        ProjectId = projectId;
        Number = number;
        Type = type;
        Title = title.Trim();
        ReporterId = reporterId;
        ParentId = parentId;
    }

    public Guid TeamId { get; private set; }
    public Guid ProjectId { get; private set; }

    /// <summary>Số thứ tự trong dự án (dùng chung bộ đếm với Bug). Mã hiển thị: {Project.Key}-{Number}.</summary>
    public int Number { get; private set; }

    public WorkItemType Type { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public WorkItemStatus Status { get; private set; } = WorkItemStatus.Todo;
    public Priority Priority { get; private set; } = Priority.Medium;

    public Guid ReporterId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid? ReviewerId { get; private set; }
    public Guid? SprintId { get; private set; }

    public decimal? EstimateHours { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Project Project { get; private set; } = null!;
    public WorkItem? Parent { get; private set; }
    public Sprint? Sprint { get; private set; }
    public AppUser Reporter { get; private set; } = null!;
    public AppUser? Assignee { get; private set; }
    public AppUser? Reviewer { get; private set; }

    public void UpdateDetails(string title, string? description, Priority priority,
        decimal? estimateHours, DateOnly? dueDate)
    {
        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        EstimateHours = estimateHours;
        DueDate = dueDate;
    }

    public void Assign(Guid? assigneeId) => AssigneeId = assigneeId;

    public void SetReviewer(Guid? reviewerId) => ReviewerId = reviewerId;

    public void MoveToSprint(Guid? sprintId) => SprintId = sprintId;

    public void ChangeStatus(WorkItemStatus to, DateTimeOffset now)
    {
        WorkItemWorkflow.EnsureCanTransition(Status, to);

        CompletedAt = to == WorkItemStatus.Done ? now : null;
        Status = to;
    }
}
