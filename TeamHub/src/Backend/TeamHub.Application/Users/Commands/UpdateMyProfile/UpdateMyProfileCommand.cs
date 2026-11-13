using Mediator;

namespace TeamHub.Application.Users.Commands.UpdateMyProfile;

/// <summary>Người dùng tự sửa họ tên và ảnh đại diện (AvatarUrl null để xóa ảnh).</summary>
public sealed record UpdateMyProfileCommand(string FullName, string? AvatarUrl) : ICommand<Unit>;
