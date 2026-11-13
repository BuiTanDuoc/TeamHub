using TeamHub.Domain.Bugs;
using TeamHub.Domain.Notifications;
using TeamHub.Domain.WorkItems;

namespace TeamHub.Application.Notifications;

/// <summary>
/// Tạo thông báo cùng giao dịch với nghiệp vụ gốc, rồi đẩy (realtime + push điện thoại) sau khi đã lưu thành công.
/// Cách dùng trong handler: Stage... → SaveChangesAsync → PublishStagedAsync.
/// Mỗi người nhận được lọc theo NotificationPreference của họ (mặc định bật tất cả kênh).
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>Thêm thông báo về task vào DbContext (chưa lưu). Bỏ qua người gây ra sự kiện và trùng lặp.</summary>
    Task StageTaskEventAsync(WorkItem task, Guid actorId, NotificationType type, string? detail,
        IEnumerable<Guid?> recipients, CancellationToken cancellationToken);

    /// <summary>Thêm thông báo về bug vào DbContext (chưa lưu). Bỏ qua người gây ra sự kiện và trùng lặp.</summary>
    Task StageBugEventAsync(Bug bug, Guid actorId, NotificationType type, string? detail,
        IEnumerable<Guid?> recipients, CancellationToken cancellationToken);

    /// <summary>Đẩy các thông báo đã lưu. Lỗi khi đẩy không làm hỏng nghiệp vụ gốc.</summary>
    Task PublishStagedAsync(CancellationToken cancellationToken);
}
