using FluentValidation;

namespace TeamHub.Application.Comments.Commands.AddComment;

public sealed class AddCommentValidator : AbstractValidator<AddCommentCommand>
{
    // 5000 khớp độ dài cột Body
    // Tối đa người được nhắc trong một bình luận, tránh dùng @mention để gửi thông báo hàng loạt
    private const int MaxMentions = 20;

    public AddCommentValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.MentionedUserIds).Must(ids => ids is null || ids.Count <= MaxMentions)
            .WithMessage($"Chỉ được nhắc tối đa {MaxMentions} người trong một bình luận.");
        RuleForEach(x => x.MentionedUserIds).NotEmpty();
    }
}
