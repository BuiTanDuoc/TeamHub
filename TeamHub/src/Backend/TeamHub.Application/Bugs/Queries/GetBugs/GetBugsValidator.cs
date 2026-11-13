using FluentValidation;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;

namespace TeamHub.Application.Bugs.Queries.GetBugs;

public sealed class GetBugsValidator : AbstractValidator<GetBugsQuery>
{
    public GetBugsValidator(IOptions<PagingOptions> options)
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, options.Value.MaxPageSize);
        RuleFor(x => x.Cursor).GreaterThanOrEqualTo(1).When(x => x.Cursor.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Severity).IsInEnum().When(x => x.Severity.HasValue);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
