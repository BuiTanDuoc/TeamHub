using TeamHub.Domain.Teams;

namespace TeamHub.Application.Common.Abstractions;

/// <summary>
/// Command/query cần user là thành viên của team với vai trò tối thiểu. AuthorizationBehavior kiểm tra
/// trước khi handler chạy, nên handler không tự check quyền. SystemAdmin được bỏ qua kiểm tra này.
/// </summary>
public interface IRequireTeamRole
{
    Guid TeamId { get; }

    /// <summary>So sánh theo thứ tự enum (Viewer &lt; Tester &lt; Developer &lt; TeamLead); quyền riêng của Tester/Developer kiểm tra tường minh ở nghiệp vụ.</summary>
    TeamRole MinimumRole { get; }
}

/// <summary>Chỉ SystemAdmin được thực hiện.</summary>
public interface IRequireSystemAdmin { }
