using TeamHub.Domain.Projects;

namespace TeamHub.Api.Endpoints;

// TeamId/ProjectId/UserId lấy từ route nên không nằm trong body.
public sealed record CreateProjectRequest(
    string Name, string Key, string? Description, DateOnly? StartDate, DateOnly? EndDate, bool UseSprints = false);

public sealed record UpdateProjectRequest(
    string Name, string? Description, DateOnly? StartDate, DateOnly? EndDate, bool UseSprints);

public sealed record ChangeProjectStatusRequest(ProjectStatus Status);

public sealed record AddProjectMemberRequest(Guid UserId);
