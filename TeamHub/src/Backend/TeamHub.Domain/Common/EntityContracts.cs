namespace TeamHub.Domain.Common;

/// <summary>Entity thuộc về một team. Dùng cho global query filter theo team.</summary>
public interface ITeamScoped
{
    Guid TeamId { get; }
}

/// <summary>Các trường audit được AuditInterceptor điền tự động.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}

/// <summary>Xóa mềm: AuditInterceptor đổi lệnh DELETE thành cập nhật các cờ này.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
