using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Commands.DeleteProject;

/// <summary>Xóa mềm dự án (AuditInterceptor đổi lệnh DELETE thành đánh dấu IsDeleted).</summary>
public sealed record DeleteProjectCommand(Guid TeamId, Guid ProjectId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
