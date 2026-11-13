using FluentValidation;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;

namespace TeamHub.Application.Tasks.Queries.GetTasks;

public sealed class GetTasksValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksValidator(IOptions<PagingOptions> options)
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, options.Value.MaxPageSize);
        RuleFor(x => x.Cursor).GreaterThanOrEqualTo(1).When(x => x.Cursor.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
