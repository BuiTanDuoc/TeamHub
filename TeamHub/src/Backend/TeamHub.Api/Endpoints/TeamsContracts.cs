using TeamHub.Domain.Teams;

namespace TeamHub.Api.Endpoints;

// TeamId/UserId lấy từ route nên không nằm trong body. Validate bằng FluentValidation ở Application.
public sealed record CreateTeamRequest(string Name, string Code, string? Description, Guid LeadUserId);

public sealed record UpdateTeamRequest(string Name, string? Description);

public sealed record SetTeamActiveRequest(bool IsActive);

public sealed record AddTeamMemberRequest(string Email, TeamRole Role, decimal? WeeklyCapacityHours);

public sealed record ChangeTeamMemberRoleRequest(TeamRole Role);

public sealed record UpdateTeamMemberCapacityRequest(decimal WeeklyCapacityHours);

public sealed record IdResponse(Guid Id);
