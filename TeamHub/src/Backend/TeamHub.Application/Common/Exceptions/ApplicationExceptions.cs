namespace TeamHub.Application.Common.Exceptions;

/// <summary>Đăng nhập/refresh thất bại. Api map sang 401.</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);

/// <summary>Không có quyền với tài nguyên (ví dụ không thuộc team). Api map sang 403.</summary>
public sealed class ForbiddenException(string message) : Exception(message);

/// <summary>Không tìm thấy (hoặc không nhìn thấy do query filter). Api map sang 404.</summary>
public sealed class NotFoundException(string name, object key)
    : Exception($"Không tìm thấy {name} ({key}).");
