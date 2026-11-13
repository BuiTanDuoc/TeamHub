using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Projects.Commands.CreateProject;

/// <summary>Tạo dự án trong team. Key là tiền tố mã task/bug (vd: TH-123), duy nhất trong team. Trả về Id dự án.</summary>
public sealed record CreateProjectCommand(
    Guid TeamId, string Name, string Key, string? Description, DateOnly? StartDate, DateOnly? EndDate, bool UseSprints)
    : ICommand<Guid>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.TeamLead;
}
