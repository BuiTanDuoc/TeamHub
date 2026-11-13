using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Models;
using TeamHub.Domain.Bugs;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Bugs.Queries.GetBugs;

/// <summary>Danh sách bug của dự án, mới nhất trước. Keyset pagination theo Number (Cursor = Number phần tử cuối trang trước).</summary>
public sealed record GetBugsQuery(
    Guid TeamId, Guid ProjectId, BugStatus? Status, BugSeverity? Severity, Guid? AssigneeId,
    string? Search, int? Cursor, int PageSize)
    : IQuery<CursorPagedResult<BugDto>>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Viewer;
}
