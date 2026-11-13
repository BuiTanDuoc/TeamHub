using Mediator;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Notifications.Commands.PurgeOldNotifications;

public sealed class PurgeOldNotificationsHandler(IAppDbContext db) : ICommandHandler<PurgeOldNotificationsCommand, int>
{
    public async ValueTask<int> Handle(PurgeOldNotificationsCommand command, CancellationToken cancellationToken)
    {
        // Notification không có query filter nên không cần IgnoreQueryFilters khi chạy ngoài HTTP request
        var total = 0;

        while (true)
        {
            // ExecuteDelete không hỗ trợ Take, nên chọn Id của một lô rồi xóa theo Id để không khóa bảng lâu
            var ids = await db.Notifications
                .Where(n => (n.IsRead && n.CreatedAt < command.ReadBefore) || n.CreatedAt < command.AnyBefore)
                .OrderBy(n => n.CreatedAt)
                .Select(n => n.Id)
                .Take(command.BatchSize)
                .ToListAsync(cancellationToken);

            if (ids.Count == 0)
                break;

            total += await db.Notifications
                .Where(n => ids.Contains(n.Id))
                .ExecuteDeleteAsync(cancellationToken);

            if (ids.Count < command.BatchSize)
                break;
        }

        return total;
    }
}
