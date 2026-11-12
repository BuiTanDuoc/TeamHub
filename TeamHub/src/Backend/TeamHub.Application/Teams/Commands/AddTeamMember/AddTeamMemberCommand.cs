using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.AddTeamMember;

/// <summary>Thêm user đã có tài khoản vào team theo email. Trả về UserId của thành viên.</summary>
public sealed record AddTeamMemberCommand(Guid TeamId, string Email, TeamRole Role, decimal? WeeklyCapacityHours)
    : ICommand<Guid>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
