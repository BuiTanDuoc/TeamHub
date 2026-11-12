using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Common;
using TeamHub.Domain.Projects;
using TeamHub.Domain.Teams;
using TeamHub.Domain.Users;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<BugStatusHistory> BugStatusHistories => Set<BugStatusHistory>();

    // Dùng property trên context để EF tham số hóa theo từng request, không "đóng băng" giá trị vào model cache.
    private Guid? CurrentUserId => currentUser.UserId;
    private bool IsSystemAdmin => currentUser.IsSystemAdmin;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // --- Lọc theo team: chỉ thấy dữ liệu của các team mà user là thành viên (SystemAdmin thấy tất cả).
        // TeamMember là bảng nguồn nên không gắn filter (tránh đệ quy).
        builder.Entity<Team>().HasQueryFilter(QueryFilterNames.TeamScope,
            t => IsSystemAdmin || TeamMembers.Any(m => m.TeamId == t.Id && m.UserId == CurrentUserId));

        ApplyTeamScope<Project>(builder);
        ApplyTeamScope<ProjectMember>(builder);
        ApplyTeamScope<Sprint>(builder);
        ApplyTeamScope<WorkItem>(builder);
        ApplyTeamScope<Bug>(builder);
        ApplyTeamScope<BugStatusHistory>(builder);

        // --- Xóa mềm
        ApplySoftDelete<Project>(builder);
        ApplySoftDelete<WorkItem>(builder);
        ApplySoftDelete<Bug>(builder);
    }

    private void ApplyTeamScope<T>(ModelBuilder builder) where T : class, ITeamScoped =>
        builder.Entity<T>().HasQueryFilter(QueryFilterNames.TeamScope,
            e => IsSystemAdmin || TeamMembers.Any(m => m.TeamId == e.TeamId && m.UserId == CurrentUserId));

    private static void ApplySoftDelete<T>(ModelBuilder builder) where T : class, ISoftDeletable =>
        builder.Entity<T>().HasQueryFilter(QueryFilterNames.SoftDelete, e => !e.IsDeleted);
}
