using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Commands.AddProjectMember;

/// <summary>Thêm thành viên team vào dự án (người được phép nhận task/bug của dự án).</summary>
public sealed record AddProjectMemberCommand(Guid TeamId, Guid ProjectId, Guid UserId) : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
