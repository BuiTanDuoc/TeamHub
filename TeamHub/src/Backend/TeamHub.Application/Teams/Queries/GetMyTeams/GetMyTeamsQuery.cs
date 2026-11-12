using Mediator;
using TeamHub.Application.Common.Models;

namespace TeamHub.Application.Teams.Queries.GetMyTeams;

/// <summary>Các team của user hiện tại (SystemAdmin thấy tất cả nhờ query filter).</summary>
public sealed record GetMyTeamsQuery(int Page, int PageSize) : IQuery<PagedResult<TeamDto>>, IPagedQuery;
