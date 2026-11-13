using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Bugs;

namespace TeamHub.Application.Bugs.Commands.CreateBug;

public sealed class CreateBugHandler(
    IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider, IOptions<BugOptions> options)
    : ICommandHandler<CreateBugCommand, CreatedBugDto>
{
    // Số lần thử lại khi nhiều người cùng tạo task/bug và tranh bộ đếm Number của dự án
    private const int MaxAttempts = 5;

    public async ValueTask<CreatedBugDto> Handle(CreateBugCommand command, CancellationToken cancellationToken)
    {
        var reporterId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");
        var now = timeProvider.GetUtcNow();

        for (var attempt = 1; ; attempt++)
        {
            var project = await db.Projects
                .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
                ?? throw new NotFoundException("Dự án", command.ProjectId);

            var number = project.ReserveItemNumber();

            var bug = new Bug(command.TeamId, command.ProjectId, number, command.Title, command.Severity,
                command.Priority, command.Environment, command.StepsToReproduce, command.ExpectedResult,
                command.ActualResult, reporterId, now);
            bug.Update(command.Title, command.Severity, command.Priority, command.Environment,
                command.AffectedVersion, command.StepsToReproduce, command.ExpectedResult, command.ActualResult,
                command.DeviceInfo);
            bug.SetSlaDueAt(options.Value.SlaFor(command.Severity) is { } sla ? now + sla : null);

            db.Bugs.Add(bug);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return new CreatedBugDto(bug.Id, number, $"{project.Key}-{number}");
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxAttempts)
            {
                // RowVersion của Project đã đổi: bỏ trạng thái đang theo dõi, đọc lại bộ đếm rồi cấp số mới
                db.ClearChangeTracker();
            }
        }
    }
}
