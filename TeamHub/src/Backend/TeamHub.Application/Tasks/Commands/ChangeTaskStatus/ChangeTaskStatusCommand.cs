using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.ChangeTaskStatus;

/// <summary>Kéo thẻ sang cột khác. Chuyển không hợp lệ → InvalidTransitionException (409).</summary>
public sealed record ChangeTaskStatusCommand(Guid TeamId, Guid ProjectId, Guid TaskId, WorkItemStatus Status)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
