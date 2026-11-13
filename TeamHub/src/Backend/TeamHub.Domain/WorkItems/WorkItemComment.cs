using TeamHub.Domain.Common;
using TeamHub.Domain.Exceptions;
using TeamHub.Domain.Users;

namespace TeamHub.Domain.WorkItems;

/// <summary>Bình luận trên task. Thời điểm sửa lấy từ UpdatedAt (AuditInterceptor); xóa là xóa mềm.</summary>
public sealed class WorkItemComment : AuditableEntity, ITeamScoped, ISoftDeletable
{
    private WorkItemComment() { } // EF Core

    public WorkItemComment(Guid teamId, Guid workItemId, Guid authorId, string body)
    {
        TeamId = teamId;
        WorkItemId = workItemId;
        AuthorId = authorId;
        Body = Normalize(body);
    }

    /// <summary>Phi chuẩn hóa từ WorkItem để áp global query filter theo team.</summary>
    public Guid TeamId { get; private set; }

    public Guid WorkItemId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public AppUser Author { get; private set; } = null!;

    public void Edit(string body) => Body = Normalize(body);

    private static string Normalize(string body)
    {
        var trimmed = body?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("Nội dung bình luận không được để trống.");
        return trimmed;
    }
}
