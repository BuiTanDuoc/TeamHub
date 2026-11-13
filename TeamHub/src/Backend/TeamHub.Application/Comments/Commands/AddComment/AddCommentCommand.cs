using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Comments.Commands.AddComment;

/// <summary>Thêm bình luận vào task. Trả về Id bình luận.</summary>
public sealed record AddCommentCommand(Guid TeamId, Guid ProjectId, Guid TaskId, string Body)
    : ICommand<Guid>, IRequireTeamRole
{
    // Mọi vai trò trừ Viewer
    public TeamRole MinimumRole => TeamRole.Tester;
}
