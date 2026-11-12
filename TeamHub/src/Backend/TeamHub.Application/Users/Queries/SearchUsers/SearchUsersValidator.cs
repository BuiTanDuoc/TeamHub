using FluentValidation;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Users.Queries.SearchUsers;

public sealed class SearchUsersValidator : PagedQueryValidator<SearchUsersQuery>
{
    public SearchUsersValidator(IOptions<PagingOptions> options) : base(options) =>
        RuleFor(x => x.Search).MaximumLength(100);
}
