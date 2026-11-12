using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Teams.Commands.CreateTeam;

/// <summary>Tạo team mới và chỉ định TeamLead đầu tiên. Trả về Id của team.</summary>
public sealed record CreateTeamCommand(string Name, string Code, string? Description, Guid LeadUserId)
    : ICommand<Guid>, IRequireSystemAdmin;
