using TeamHub.Domain.Projects;

namespace TeamHub.Application.Projects;

public sealed record ProjectDto(
    Guid Id, Guid TeamId, string Name, string Key, string? Description, ProjectStatus Status,
    DateOnly? StartDate, DateOnly? EndDate, bool UseSprints, int MemberCount, DateTimeOffset CreatedAt);

public sealed record ProjectMemberDto(Guid UserId, string Email, string FullName, string? AvatarUrl, DateTimeOffset AddedAt);
