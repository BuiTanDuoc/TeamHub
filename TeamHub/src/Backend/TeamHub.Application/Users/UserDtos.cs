using TeamHub.Domain.Teams;

namespace TeamHub.Application.Users;

public sealed record UserSummaryDto(
    Guid Id, string Email, string FullName, string? AvatarUrl, bool IsActive, bool IsSystemAdmin);

public sealed record UserTeamDto(Guid TeamId, string TeamName, TeamRole Role);

public sealed record UserDetailDto(
    Guid Id, string Email, string FullName, string? AvatarUrl, bool IsActive, bool IsSystemAdmin,
    DateTimeOffset CreatedAt, IReadOnlyList<UserTeamDto> Teams);
