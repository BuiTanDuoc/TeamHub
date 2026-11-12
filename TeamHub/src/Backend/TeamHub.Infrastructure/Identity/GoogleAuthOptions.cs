namespace TeamHub.Infrastructure.Identity;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "Auth:Google";

    /// <summary>Danh sách OAuth client id hợp lệ (web, Android, iOS). ID token phải có audience thuộc danh sách này.</summary>
    public string[] ClientIds { get; set; } = [];

    /// <summary>Tự tạo tài khoản khi đăng nhập Google lần đầu (tài khoản mới chưa thuộc team nào).</summary>
    public bool AutoCreateUsers { get; set; } = true;

    /// <summary>Nếu có giá trị: chỉ chấp nhận email thuộc các tên miền này (vd: congty.vn).</summary>
    public string[] AllowedDomains { get; set; } = [];
}
