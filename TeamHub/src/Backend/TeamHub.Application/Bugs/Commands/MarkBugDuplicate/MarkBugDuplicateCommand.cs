using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Commands.MarkBugDuplicate;

/// <summary>Đánh dấu bug trùng với một bug khác trong cùng dự án (chỉ TeamLead theo BugWorkflow). Lý do bắt buộc.</summary>
public sealed record MarkBugDuplicateCommand(Guid TeamId, Guid ProjectId, Guid BugId, Guid OriginalBugId, string Note)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
