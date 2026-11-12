using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Teams.Queries.GetTeamMembers;

public sealed class GetTeamMembersValidator(IOptions<PagingOptions> options)
    : PagedQueryValidator<GetTeamMembersQuery>(options);
