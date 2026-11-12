using TeamHub.Domain.Common;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Domain.Projects;

public sealed class Sprint : AuditableEntity, ITeamScoped
{
    private Sprint() { } // EF Core

    public Sprint(Guid teamId, Guid projectId, string name, DateOnly startDate, DateOnly endDate, string? goal)
    {
        if (endDate < startDate)
            throw new DomainException("Ngày kết thúc sprint phải sau hoặc bằng ngày bắt đầu.");

        TeamId = teamId;
        ProjectId = projectId;
        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Goal = goal?.Trim();
    }

    public Guid TeamId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Goal { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public SprintStatus Status { get; private set; } = SprintStatus.Planned;

    public Project Project { get; private set; } = null!;

    public void Start()
    {
        if (Status != SprintStatus.Planned)
            throw new InvalidTransitionException("Sprint", Status.ToString(), SprintStatus.Active.ToString());
        Status = SprintStatus.Active;
    }

    public void Close()
    {
        if (Status != SprintStatus.Active)
            throw new InvalidTransitionException("Sprint", Status.ToString(), SprintStatus.Closed.ToString());
        Status = SprintStatus.Closed;
    }
}
