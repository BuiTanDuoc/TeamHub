using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Notifications;

/// <summary>
/// Tạo thông báo cùng giao dịch với nghiệp vụ gốc, rồi đẩy realtime sau khi đã lưu thành công.
/// Cách dùng trong handler: StageTaskEventAsync → SaveChangesAsync → PublishStagedAsync.
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>Thêm thông báo vào DbContext (chưa lưu). Bỏ qua người gây ra sự kiện và trùng lặp.</summary>
    Task StageTaskEventAsync(WorkItem task, Guid actorId, NotificationType type, string? detail,
        IEnumerable<Guid?> recipients, CancellationToken cancellationToken);

    /// <summary>Đẩy các thông báo đã lưu qua realtime. Lỗi đẩy không làm hỏng nghiệp vụ gốc.</summary>
    Task PublishStagedAsync(CancellationToken cancellationToken);
}
