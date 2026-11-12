using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams;

/// <summary>Mapping thủ công dạng projection để EF chỉ select các cột cần thiết.</summary>
public static class TeamMappings
{
    public static IQueryable<TeamDto> ToDtos(this IQueryable<Team> query) =>
        query.Select(t => new TeamDto(
            t.Id, t.Name, t.Code, t.Description, t.IsActive, t.Members.Count(), t.CreatedAt));

    public static IQueryable<TeamMemberDto> ToDtos(this IQueryable<TeamMember> query) =>
        query.Select(m => new TeamMemberDto(
            m.UserId, m.User.Email ?? string.Empty, m.User.FullName, m.User.AvatarUrl,
            m.Role, m.WeeklyCapacityHours, m.JoinedAt));
}
