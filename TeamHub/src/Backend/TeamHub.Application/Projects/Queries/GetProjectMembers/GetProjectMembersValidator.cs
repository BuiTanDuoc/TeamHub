using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Projects.Queries.GetProjectMembers;

public sealed class GetProjectMembersValidator(IOptions<PagingOptions> options)
    : PagedQueryValidator<GetProjectMembersQuery>(options);
