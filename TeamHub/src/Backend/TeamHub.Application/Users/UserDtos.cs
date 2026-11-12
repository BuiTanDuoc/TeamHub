namespace TeamHub.Application.Users;

public sealed record UserSummaryDto(Guid Id, string Email, string FullName, string? AvatarUrl);
