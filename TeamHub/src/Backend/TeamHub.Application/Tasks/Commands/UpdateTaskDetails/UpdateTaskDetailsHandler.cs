using System.Globalization;
using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Tasks.Commands.UpdateTaskDetails;

public sealed class UpdateTaskDetailsHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<UpdateTaskDetailsCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateTaskDetailsCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Chưa đăng nhập.");

        var item = await db.WorkItems
            .SingleOrDefaultAsync(w => w.Id == command.TaskId && w.TeamId == command.TeamId
                && w.ProjectId == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Task", command.TaskId);

        var oldEstimate = item.EstimateHours;
        item.UpdateDetails(command.Title, command.Description, command.Priority, command.EstimateHours, command.DueDate);

        if (oldEstimate != item.EstimateHours)
        {
            db.WorkItemHistories.Add(new WorkItemHistory(item.TeamId, item.Id, WorkItemField.Estimate,
                Format(oldEstimate), Format(item.EstimateHours), userId, timeProvider.GetUtcNow()));
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private static string? Format(decimal? hours) => hours?.ToString("0.##", CultureInfo.InvariantCulture);
}
