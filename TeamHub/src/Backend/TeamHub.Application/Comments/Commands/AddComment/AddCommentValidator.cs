using FluentValidation;

namespace TeamHub.Application.Comments.Commands.AddComment;

public sealed class AddCommentValidator : AbstractValidator<AddCommentCommand>
{
    // 5000 khớp độ dài cột Body
    public AddCommentValidator() => RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
}
