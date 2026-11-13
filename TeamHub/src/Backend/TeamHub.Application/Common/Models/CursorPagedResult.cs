namespace TeamHub.Application.Common.Models;

/// <summary>Kết quả keyset pagination: NextCursor null nghĩa là đã hết dữ liệu.</summary>
public sealed record CursorPagedResult<T>(IReadOnlyList<T> Items, int? NextCursor);
