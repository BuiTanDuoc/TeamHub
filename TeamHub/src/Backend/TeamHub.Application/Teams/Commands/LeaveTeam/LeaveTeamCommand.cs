using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.LeaveTeam;

/// <summary>Thành viên tự rời team (mọi vai trò). TeamLead cuối cùng không được rời.</summary>
public sealed record LeaveTeamCommand(Guid TeamId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
