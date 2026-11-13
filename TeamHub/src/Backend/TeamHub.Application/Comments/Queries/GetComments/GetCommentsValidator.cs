using Microsoft.Extensions.Options;
using TeamHub.Application.Common;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Comments.Queries.GetComments;

public sealed class GetCommentsValidator(IOptions<PagingOptions> options) : PagedQueryValidator<GetCommentsQuery>(options);
