using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Teams;

namespace TeamHub.Domain.Bugs;

/// <summary>Nơi duy nhất định nghĩa chuyển trạng thái hợp lệ và vai trò được phép thực hiện.</summary>
public static class BugWorkflow
{
    private static readonly Dictionary<BugStatus, BugStatus[]> Transitions = new()
    {
        [BugStatus.New] = [BugStatus.Confirmed, BugStatus.Duplicate, BugStatus.CannotReproduce, BugStatus.WontFix],
        [BugStatus.Confirmed] = [BugStatus.Assigned, BugStatus.Duplicate, BugStatus.WontFix],
        [BugStatus.Assigned] = [BugStatus.InProgress, BugStatus.WontFix],
        [BugStatus.InProgress] = [BugStatus.Fixed, BugStatus.Assigned, BugStatus.CannotReproduce, BugStatus.WontFix],
        [BugStatus.Fixed] = [BugStatus.ReadyToRetest],
        [BugStatus.ReadyToRetest] = [BugStatus.Verified, BugStatus.Reopened],
        [BugStatus.Verified] = [BugStatus.Closed, BugStatus.Reopened],
        [BugStatus.Reopened] = [BugStatus.Assigned, BugStatus.InProgress],
        [BugStatus.Closed] = [BugStatus.Reopened],
        [BugStatus.WontFix] = [BugStatus.Reopened],
        [BugStatus.Duplicate] = [BugStatus.Reopened],
        [BugStatus.CannotReproduce] = [BugStatus.Reopened]
    };

    // Vai trò được phép chuyển TỚI trạng thái đích. TeamLead luôn được phép.
    // Trạng thái không có trong bảng (WontFix, Duplicate) chỉ TeamLead được đặt.
    private static readonly Dictionary<BugStatus, TeamRole[]> RolesByTarget = new()
    {
        [BugStatus.Confirmed] = [TeamRole.Tester],
        [BugStatus.Assigned] = [TeamRole.Developer],
        [BugStatus.InProgress] = [TeamRole.Developer],
        [BugStatus.Fixed] = [TeamRole.Developer],
        [BugStatus.ReadyToRetest] = [TeamRole.Developer],
        [BugStatus.CannotReproduce] = [TeamRole.Developer],
        [BugStatus.Verified] = [TeamRole.Tester],
        [BugStatus.Closed] = [TeamRole.Tester],
        [BugStatus.Reopened] = [TeamRole.Tester]
    };

    public static bool CanTransition(BugStatus from, BugStatus to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static void EnsureCanTransition(BugStatus from, BugStatus to)
    {
        if (!CanTransition(from, to))
            throw new InvalidTransitionException("Bug", from.ToString(), to.ToString());
    }

    public static bool IsRoleAllowed(BugStatus to, TeamRole role) =>
        role == TeamRole.TeamLead
        || (RolesByTarget.TryGetValue(to, out var roles) && roles.Contains(role));

    /// <summary>Các trạng thái bắt buộc phải ghi lý do.</summary>
    public static bool RequiresNote(BugStatus to) =>
        to is BugStatus.Reopened or BugStatus.WontFix or BugStatus.Duplicate or BugStatus.CannotReproduce;

    /// <summary>Bug đã xử lý xong (hoặc bị loại bỏ), không còn tính vào SLA.</summary>
    public static bool IsFinal(BugStatus status) =>
        status is BugStatus.Closed or BugStatus.WontFix or BugStatus.Duplicate or BugStatus.CannotReproduce;
}
