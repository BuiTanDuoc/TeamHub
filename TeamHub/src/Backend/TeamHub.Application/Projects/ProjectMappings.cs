using TeamHub.Domain.Projects;

namespace TeamHub.Application.Projects;

/// <summary>Mapping thủ công dạng projection để EF chỉ select các cột cần thiết.</summary>
public static class ProjectMappings
{
    public static IQueryable<ProjectDto> ToDtos(this IQueryable<Project> query) =>
        query.Select(p => new ProjectDto(
            p.Id, p.TeamId, p.Name, p.Key, p.Description, p.Status,
            p.StartDate, p.EndDate, p.UseSprints, p.Members.Count(), p.CreatedAt));

    public static IQueryable<ProjectMemberDto> ToDtos(this IQueryable<ProjectMember> query) =>
        query.Select(m => new ProjectMemberDto(
            m.UserId, m.User.Email ?? string.Empty, m.User.FullName, m.User.AvatarUrl, m.AddedAt));
}
