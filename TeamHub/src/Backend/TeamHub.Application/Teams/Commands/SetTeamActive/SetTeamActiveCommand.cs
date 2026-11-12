using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Teams.Commands.SetTeamActive;

/// <summary>Ngừng/kích hoạt lại team. Chỉ SystemAdmin vì ảnh hưởng toàn bộ dự án của team.</summary>
public sealed record SetTeamActiveCommand(Guid TeamId, bool IsActive) : ICommand<Unit>, IRequireSystemAdmin;
