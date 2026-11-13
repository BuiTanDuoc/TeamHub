using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Tasks.Commands.DeleteTask;

/// <summary>Xóa mềm task (AuditInterceptor đổi DELETE thành đánh dấu IsDeleted). Số thứ tự không được dùng lại.</summary>
public sealed record DeleteTaskCommand(Guid TeamId, Guid ProjectId, Guid TaskId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
