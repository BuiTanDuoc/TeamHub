using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Comments.Commands.AddComment;

/// <summary>
/// Thêm bình luận vào task. MentionedUserIds là những người được @nhắc đến (client gửi Id, server không tự đoán
/// từ nội dung); họ phải là thành viên của team. Trả về Id bình luận.
/// </summary>
public sealed record AddCommentCommand(
    Guid TeamId, Guid ProjectId, Guid TaskId, string Body, IReadOnlyList<Guid>? MentionedUserIds = null)
    : ICommand<Guid>, IRequireTeamRole
{
    // Mọi vai trò trừ Viewer
    public TeamRole MinimumRole => TeamRole.Tester;
}
