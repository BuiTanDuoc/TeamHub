using TeamHub.Domain.Common;
using TeamHub.Domain.Users;

namespace TeamHub.Domain.Projects;

public sealed class ProjectMember : ITeamScoped
{
    private ProjectMember() { } // EF Core

    internal ProjectMember(Guid teamId, Guid projectId, Guid userId, DateTimeOffset addedAt)
    {
        TeamId = teamId;
        ProjectId = projectId;
        UserId = userId;
        AddedAt = addedAt;
    }

    /// <summary>Phi chuẩn hóa từ Project để áp global query filter theo team.</summary>
    public Guid TeamId { get; private set; }

    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    public Project Project { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;
}
