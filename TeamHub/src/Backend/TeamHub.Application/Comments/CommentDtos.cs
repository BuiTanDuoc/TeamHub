namespace TeamHub.Application.Comments;

public sealed record CommentDto(
    Guid Id, Guid AuthorId, string AuthorName, string? AuthorAvatarUrl, string Body,
    DateTimeOffset CreatedAt, DateTimeOffset? EditedAt);
