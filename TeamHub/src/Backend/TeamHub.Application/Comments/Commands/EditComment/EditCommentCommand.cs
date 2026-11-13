using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Comments.Commands.EditComment;

/// <summary>Sửa bình luận; chỉ tác giả được sửa.</summary>
public sealed record EditCommentCommand(Guid TeamId, Guid ProjectId, Guid TaskId, Guid CommentId, string Body)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
