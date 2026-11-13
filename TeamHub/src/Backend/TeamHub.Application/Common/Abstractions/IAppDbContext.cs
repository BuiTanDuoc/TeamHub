using Microsoft.EntityFrameworkCore;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Projects;
using TeamHub.Domain.Teams;
using TeamHub.Domain.Users;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Common.Abstractions;

public interface IAppDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<Sprint> Sprints { get; }
    DbSet<WorkItem> WorkItems { get; }
    DbSet<WorkItemHistory> WorkItemHistories { get; }
    DbSet<WorkItemComment> WorkItemComments { get; }
    DbSet<Bug> Bugs { get; }
    DbSet<BugStatusHistory> BugStatusHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Bỏ mọi entity đang theo dõi; dùng khi thử lại sau DbUpdateConcurrencyException.</summary>
    void ClearChangeTracker();
}
