using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Commands.UpdateProject;

/// <summary>Cập nhật thông tin dự án (không đổi Key vì Key đã nằm trong mã task/bug).</summary>
public sealed record UpdateProjectCommand(
    Guid TeamId, Guid ProjectId, string Name, string? Description, DateOnly? StartDate, DateOnly? EndDate, bool UseSprints)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
