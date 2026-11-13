using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Bugs;

namespace TeamHub.Application.Bugs.Queries.GetBug;

public sealed class GetBugHandler(IAppDbContext db, ICurrentUser currentUser) : IQueryHandler<GetBugQuery, BugDetailDto>
{
    public async ValueTask<BugDetailDto> Handle(GetBugQuery query, CancellationToken cancellationToken)
    {
        var role = await db.ResolveActorRoleAsync(currentUser, query.TeamId, cancellationToken);

        var bug = await db.Bugs.AsNoTracking()
            .Where(b => b.Id == query.BugId && b.TeamId == query.TeamId && b.ProjectId == query.ProjectId)
            .ToDetails()
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Bug", query.BugId);

        // Chỉ liệt kê chuyển hợp lệ VÀ vai trò của người xem được phép, để UI không hiện nút sẽ bị từ chối
        var allowed = Enum.GetValues<BugStatus>()
            .Where(s => BugWorkflow.CanTransition(bug.Status, s) && BugWorkflow.IsRoleAllowed(s, role))
            .ToList();

        return bug with { AllowedTransitions = allowed };
    }
}
