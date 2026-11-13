using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Application.Projects.Commands.AddProjectMember;

public sealed class AddProjectMemberHandler(IAppDbContext db, TimeProvider timeProvider)
    : ICommandHandler<AddProjectMemberCommand, Unit>
{
    public async ValueTask<Unit> Handle(AddProjectMemberCommand command, CancellationToken cancellationToken)
    {
        // Project.AddMember cần danh sách thành viên đã được load
        var project = await db.Projects
            .Include(p => p.Members)
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
            ?? throw new NotFoundException("Dự án", command.ProjectId);

        var isTeamMember = await db.TeamMembers
            .AnyAsync(m => m.TeamId == command.TeamId && m.UserId == command.UserId, cancellationToken);
        if (!isTeamMember)
            throw new DomainException("Người dùng chưa thuộc team nên không thể thêm vào dự án.");

        project.AddMember(command.UserId, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
