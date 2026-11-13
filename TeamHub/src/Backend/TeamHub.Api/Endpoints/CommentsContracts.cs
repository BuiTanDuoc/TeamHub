namespace TeamHub.Api.Endpoints;

/// <summary>Sửa bình luận. Không hỗ trợ @mention khi sửa (chỉ khi thêm mới).</summary>
public sealed record CommentRequest(string Body);

/// <summary>MentionedUserIds là Id những người được @nhắc đến, phải là thành viên của team.</summary>
public sealed record AddCommentRequest(string Body, IReadOnlyList<Guid>? MentionedUserIds = null);
