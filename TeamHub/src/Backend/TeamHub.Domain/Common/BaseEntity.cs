namespace TeamHub.Domain.Common;

public abstract class BaseEntity
{
    // GUID v7: tăng dần theo thời gian nên index clustered ít bị phân mảnh hơn GUID v4
    public Guid Id { get; protected init; } = Guid.CreateVersion7();
}
