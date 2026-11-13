using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskHandler(IAppDbContext db, ICurrentUser currentUser)
    : ICommandHandler<CreateTaskCommand, CreatedTaskDto>
{
    // Số lần thử lại khi nhiều người cùng tạo task và tranh bộ đếm Number của dự án
    private const int MaxAttempts = 5;

    public async ValueTask<CreatedTaskDto> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var reporterId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        if (command.ParentId is { } parentId)
        {
            var parentType = await db.WorkItems
                .Where(w => w.Id == parentId && w.TeamId == command.TeamId && w.ProjectId == command.ProjectId)
                .Select(w => (WorkItemType?)w.Type)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Task cha", parentId);

            if (parentType == WorkItemType.SubTask)
                throw new DomainException("Sub-task không thể làm item cha.");
        }

        if (command.AssigneeId is { } assigneeId)
        {
            var isProjectMember = await db.ProjectMembers
                .AnyAsync(m => m.ProjectId == command.ProjectId && m.UserId == assigneeId, cancellationToken);
            if (!isProjectMember)
                throw new DomainException("Người được giao phải là thành viên của dự án.");
        }

        for (var attempt = 1; ; attempt++)
        {
            var project = await db.Projects
                .SingleOrDefaultAsync(p => p.Id == command.ProjectId && p.TeamId == command.TeamId, cancellationToken)
                ?? throw new NotFoundException("Dự án", command.ProjectId);

            var number = project.ReserveItemNumber();

            var item = new WorkItem(command.TeamId, command.ProjectId, number, command.Type,
                command.Title, reporterId, command.ParentId);
            item.UpdateDetails(command.Title, command.Description, command.Priority,
                command.EstimateHours, command.DueDate);
            item.Assign(command.AssigneeId);

            db.WorkItems.Add(item);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return new CreatedTaskDto(item.Id, number, $"{project.Key}-{number}");
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxAttempts)
            {
                // RowVersion của Project đã đổi: bỏ trạng thái đang theo dõi, đọc lại bộ đếm rồi cấp số mới
                db.ClearChangeTracker();
            }
        }
    }
}
