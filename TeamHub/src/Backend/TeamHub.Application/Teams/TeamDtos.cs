using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams;

public sealed record TeamDto(
    Guid Id, string Name, string Code, string? Description, bool IsActive, int MemberCount, DateTimeOffset CreatedAt);

public sealed record TeamMemberDto(
    Guid UserId, string Email, string FullName, string? AvatarUrl,
    TeamRole Role, decimal WeeklyCapacityHours, DateTimeOffset JoinedAt);
