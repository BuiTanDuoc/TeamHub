using FluentValidation;

namespace TeamHub.Application.Comments.Commands.EditComment;

public sealed class EditCommentValidator : AbstractValidator<EditCommentCommand>
{
    public EditCommentValidator() => RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
}
