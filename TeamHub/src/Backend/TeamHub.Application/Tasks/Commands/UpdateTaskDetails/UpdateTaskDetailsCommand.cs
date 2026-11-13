using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Common;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Tasks.Commands.UpdateTaskDetails;

/// <summary>Sửa tiêu đề, mô tả, độ ưu tiên, estimate, hạn chót. Đổi estimate được ghi vào lịch sử.</summary>
public sealed record UpdateTaskDetailsCommand(
    Guid TeamId, Guid ProjectId, Guid TaskId, string Title, string? Description, Priority Priority,
    decimal? EstimateHours, DateOnly? DueDate)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
