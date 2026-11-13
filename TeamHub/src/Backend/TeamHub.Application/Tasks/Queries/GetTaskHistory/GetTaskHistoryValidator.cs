using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Tasks.Queries.GetTaskHistory;

public sealed class GetTaskHistoryValidator(IOptions<PagingOptions> options)
    : PagedQueryValidator<GetTaskHistoryQuery>(options);
