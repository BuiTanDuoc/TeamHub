using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Teams.Queries.GetMyTeams;

public sealed class GetMyTeamsValidator(IOptions<PagingOptions> options)
    : PagedQueryValidator<GetMyTeamsQuery>(options);
