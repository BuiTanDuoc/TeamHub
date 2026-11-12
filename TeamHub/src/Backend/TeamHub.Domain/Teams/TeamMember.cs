using TeamHub.Domain.Common;
using TeamHub.Domain.Users;

namespace TeamHub.Domain.Teams;

/// <summary>
/// Bảng nguồn để xác định quyền truy cập team, nên KHÔNG gắn global query filter theo team
/// (tránh đệ quy). Mọi truy vấn trực tiếp phải lọc theo TeamId sau khi AuthorizationBehavior xác nhận.
/// </summary>
public sealed class TeamMember : ITeamScoped
{
    private TeamMember() { } // EF Core

    internal TeamMember(Guid teamId, Guid userId, TeamRole role, DateTimeOffset joinedAt, decimal weeklyCapacityHours)
    {
        TeamId = teamId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
        WeeklyCapacityHours = weeklyCapacityHours;
    }

    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public TeamRole Role { get; internal set; }

    /// <summary>Số giờ làm việc mỗi tuần, dùng để tính workload.</summary>
    public decimal WeeklyCapacityHours { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public Team Team { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;
}
