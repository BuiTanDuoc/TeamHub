using Mediator;
using TeamHub.Application.Common.Abstractions;

namespace TeamHub.Application.Users.Queries.GetUser;

/// <summary>Chi tiết một người dùng kèm các team họ tham gia và vai trò. Chỉ SystemAdmin.</summary>
public sealed record GetUserQuery(Guid UserId) : IQuery<UserDetailDto>, IRequireSystemAdmin;
