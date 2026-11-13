using FluentValidation;
using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Projects.Queries.GetProjects;

public sealed class GetProjectsValidator : PagedQueryValidator<GetProjectsQuery>
{
    public GetProjectsValidator(IOptions<PagingOptions> options) : base(options) =>
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
}
