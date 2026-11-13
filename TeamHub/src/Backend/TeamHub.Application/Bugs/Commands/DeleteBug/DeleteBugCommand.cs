using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Commands.DeleteBug;

/// <summary>Xóa mềm bug (AuditInterceptor đổi DELETE thành đánh dấu IsDeleted). Số thứ tự không được dùng lại.</summary>
public sealed record DeleteBugCommand(Guid TeamId, Guid ProjectId, Guid BugId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
