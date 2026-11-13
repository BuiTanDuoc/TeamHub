using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Projects;

namespace TeamHub.Application.Projects.Commands.CreateProject;

public sealed class CreateProjectHandler(IAppDbContext db) : ICommandHandler<CreateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var isActive = await db.Teams
            .Where(t => t.Id == command.TeamId)
            .Select(t => (bool?)t.IsActive)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Team", command.TeamId);
        if (!isActive)
            throw new DomainException("Team đã ngừng hoạt động, không thể tạo dự án.");

        // Query filter SoftDelete đã loại dự án đã xóa, khớp với unique index có điều kiện IsDeleted = 0
        var key = command.Key.Trim().ToUpperInvariant();
        if (await db.Projects.AnyAsync(p => p.TeamId == command.TeamId && p.Key == key, cancellationToken))
            throw new DomainException("Key dự án đã tồn tại trong team.");

        var project = new Project(command.TeamId, command.Name, key, command.Description,
            command.StartDate, command.EndDate, command.UseSprints);

        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
