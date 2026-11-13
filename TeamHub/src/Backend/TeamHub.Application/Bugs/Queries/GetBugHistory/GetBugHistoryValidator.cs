using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Bugs.Queries.GetBugHistory;

public sealed class GetBugHistoryValidator(IOptions<PagingOptions> options) : PagedQueryValidator<GetBugHistoryQuery>(options);
