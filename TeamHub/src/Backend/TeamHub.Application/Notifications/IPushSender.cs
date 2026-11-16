namespace TeamHub.Application.Notifications;

/// <summary>Một tin đẩy tới một thiết bị. Data là các giá trị chuỗi để app điều hướng khi người dùng chạm vào.</summary>
public sealed record PushMessage(string To, string Title, string Body, IReadOnlyDictionary<string, string> Data);

/// <summary>Gửi push tới thiết bị (Expo Push cho điện thoại, Firebase Cloud Messaging cho web). Triển khai ở Api.</summary>
public interface IPushSender
{
    /// <summary>Gửi một loạt tin. Trả về các token không còn hợp lệ (thiết bị đã gỡ app/thu hồi quyền) để xóa khỏi DB.</summary>
    Task<IReadOnlyList<string>> SendAsync(IReadOnlyList<PushMessage> messages, CancellationToken cancellationToken);
}
