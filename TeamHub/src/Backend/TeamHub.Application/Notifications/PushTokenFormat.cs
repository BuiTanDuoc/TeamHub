using System.Text.RegularExpressions;

namespace TeamHub.Application.Notifications;

/// <summary>
/// Phân loại token đẩy theo định dạng: Expo (điện thoại qua Expo Notifications) hay FCM (web/native qua Firebase).
/// Token không cần cột "loại" riêng vì hai định dạng không trùng nhau.
/// </summary>
public static partial class PushTokenFormat
{
    /// <summary>Độ dài tối đa, khớp cột DeviceTokens.Token.</summary>
    public const int MaxLength = 512;

    // Dạng ExponentPushToken[xxxx] hoặc ExpoPushToken[xxxx]
    [GeneratedRegex(@"^Expo(nent)?PushToken\[.+\]$")]
    private static partial Regex ExpoPattern();

    // FCM registration token: chữ, số, '-', '_', ':' và '.'; không có ký tự '[' ']' nên không lẫn với Expo
    [GeneratedRegex(@"^[A-Za-z0-9_\-:.]{20,512}$")]
    private static partial Regex FcmPattern();

    public static bool IsExpo(string token) => ExpoPattern().IsMatch(token);

    public static bool IsFcm(string token) => FcmPattern().IsMatch(token);
}
