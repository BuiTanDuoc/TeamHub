using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Teams.Commands.UpdateTeamMemberCapacity;

/// <summary>Cập nhật số giờ làm việc mỗi tuần của thành viên (dùng tính workload).</summary>
public sealed record UpdateTeamMemberCapacityCommand(Guid TeamId, Guid UserId, decimal WeeklyCapacityHours)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
