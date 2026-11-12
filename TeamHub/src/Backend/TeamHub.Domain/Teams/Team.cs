using TeamHub.Domain.Common;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Domain.Teams;

public sealed class Team : AuditableEntity
{
    private readonly List<TeamMember> _members = [];

    private Team() { } // EF Core

    public Team(string name, string code, string? description)
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        Description = description?.Trim();
    }

    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<TeamMember> Members => _members;

    public void Update(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    // Lưu ý: các hàm thao tác thành viên yêu cầu handler đã .Include(t => t.Members)

    public TeamMember AddMember(Guid userId, TeamRole role, DateTimeOffset joinedAt, decimal weeklyCapacityHours = 40m)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new DomainException("Người dùng đã là thành viên của team.");

        var member = new TeamMember(Id, userId, role, joinedAt, weeklyCapacityHours);
        _members.Add(member);
        return member;
    }

    public void ChangeMemberRole(Guid userId, TeamRole role)
    {
        var member = GetMember(userId);

        // Không cho hạ vai trò TeamLead cuối cùng
        if (member.Role == TeamRole.TeamLead && role != TeamRole.TeamLead)
            EnsureAnotherLeadExists(userId);

        member.Role = role;
    }

    public void RemoveMember(Guid userId)
    {
        var member = GetMember(userId);

        if (member.Role == TeamRole.TeamLead)
            EnsureAnotherLeadExists(userId);

        _members.Remove(member);
    }

    private TeamMember GetMember(Guid userId) =>
        _members.FirstOrDefault(m => m.UserId == userId)
        ?? throw new DomainException("Không tìm thấy thành viên trong team.");

    private void EnsureAnotherLeadExists(Guid excludedUserId)
    {
        if (!_members.Any(m => m.UserId != excludedUserId && m.Role == TeamRole.TeamLead))
            throw new DomainException("Team phải còn ít nhất một TeamLead.");
    }
}
