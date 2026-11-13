using Mediator;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Teams;

namespace TeamHub.Application.Comments.Commands.DeleteComment;

/// <summary>Xóa mềm bình luận; tác giả, TeamLead của team hoặc SystemAdmin được xóa.</summary>
public sealed record DeleteCommentCommand(Guid TeamId, Guid ProjectId, Guid TaskId, Guid CommentId)
    : ICommand<Unit>, IRequireTeamRole
{
    public TeamRole MinimumRole => TeamRole.Tester;
}
