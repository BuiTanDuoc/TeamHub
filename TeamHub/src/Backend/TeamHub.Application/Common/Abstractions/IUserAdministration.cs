namespace TeamHub.Application.Common.Abstractions;

/// <summary>Kết quả thao tác tài khoản. Errors là các thông báo lỗi (ví dụ mật khẩu không đạt chính sách) khi Succeeded = false.</summary>
public sealed record AccountOperationResult(bool Succeeded, IReadOnlyList<string> Errors, Guid? UserId = null)
{
    public static AccountOperationResult Ok(Guid? userId = null) => new(true, [], userId);

    public static AccountOperationResult Fail(params string[] errors) => new(false, errors);

    public static AccountOperationResult Fail(IEnumerable<string> errors) => new(false, errors.ToList());

    public string ErrorMessage => string.Join(" ", Errors);
}

/// <summary>
/// Các thao tác tài khoản cần UserManager của ASP.NET Identity (băm mật khẩu, chính sách mật khẩu, thu hồi phiên).
/// Triển khai ở Infrastructure để Application không phụ thuộc Identity.
/// </summary>
public interface IUserAdministration
{
    /// <summary>Tạo tài khoản đăng nhập bằng mật khẩu do quản trị viên đặt. Email phải chưa dùng.</summary>
    Task<AccountOperationResult> CreateUserAsync(
        string email, string fullName, string password, bool isSystemAdmin, CancellationToken cancellationToken);

    /// <summary>Quản trị viên đặt lại mật khẩu (không cần mật khẩu cũ) và thu hồi mọi phiên của người đó.</summary>
    Task<AccountOperationResult> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);

    /// <summary>Người dùng tự đổi mật khẩu (cần mật khẩu hiện tại) và bị thu hồi mọi phiên để đăng nhập lại.</summary>
    Task<AccountOperationResult> ChangePasswordAsync(
        Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    /// <summary>Thu hồi mọi refresh token còn hiệu lực của người dùng (buộc đăng nhập lại khi access token hết hạn).</summary>
    Task RevokeSessionsAsync(Guid userId, CancellationToken cancellationToken);
}
