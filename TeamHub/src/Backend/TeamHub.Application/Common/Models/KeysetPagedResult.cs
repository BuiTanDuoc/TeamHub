namespace TeamHub.Application.Common.Models;

/// <summary>Kết quả keyset pagination với cursor mờ (opaque): NextCursor null nghĩa là đã hết dữ liệu.</summary>
public sealed record KeysetPagedResult<T>(IReadOnlyList<T> Items, string? NextCursor);
