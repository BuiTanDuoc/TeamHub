using TeamHub.Domain.Bugs;

namespace TeamHub.Application.Bugs;

public static class BugMappings
{
    // Mã hiển thị {Project.Key}-{Number}
    public static IQueryable<BugDto> ToCards(this IQueryable<Bug> query) =>
        query.Select(b => new BugDto(
            b.Id, b.ProjectId, b.Number, b.Project.Key + "-" + b.Number.ToString(), b.Title, b.Severity, b.Priority,
            b.Status, b.Environment, b.AssigneeId, b.Assignee != null ? b.Assignee.FullName : null,
            b.SlaDueAt, b.CreatedAt));

    public static IQueryable<BugDetailDto> ToDetails(this IQueryable<Bug> query) =>
        query.Select(b => new BugDetailDto(
            b.Id, b.ProjectId, b.Number, b.Project.Key + "-" + b.Number.ToString(), b.Title, b.Severity, b.Priority,
            b.Status, b.Environment, b.AffectedVersion, b.FixedInVersion,
            b.StepsToReproduce, b.ExpectedResult, b.ActualResult, b.DeviceInfo,
            b.ReporterId, b.Reporter.FullName, b.AssigneeId, b.Assignee != null ? b.Assignee.FullName : null,
            b.LinkedWorkItemId, b.DuplicateOfBugId, b.SlaDueAt,
            b.ResolvedAt, b.ClosedAt, b.ReopenCount, b.CreatedAt));
}
