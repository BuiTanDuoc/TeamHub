using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Tasks.Commands.AssignTask;

/// <summary>Giao task cho thành viên dự án; AssigneeId null nghĩa là bỏ giao.</summary>
public sealed record AssignTaskCommand(Guid TeamId, Guid ProjectId, Guid TaskId, Guid? AssigneeId)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
