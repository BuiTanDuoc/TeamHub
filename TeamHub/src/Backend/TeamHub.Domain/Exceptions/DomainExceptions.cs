namespace TeamHub.Domain.Exceptions;

/// <summary>Vi phạm quy tắc nghiệp vụ. Api map sang 400/409 qua ExceptionHandlingMiddleware.</summary>
public class DomainException(string message) : Exception(message);

/// <summary>Chuyển trạng thái không hợp lệ theo workflow. Api map sang 409.</summary>
public sealed class InvalidTransitionException(string entity, string from, string to)
    : DomainException($"{entity}: không thể chuyển từ '{from}' sang '{to}'.");

/// <summary>Vai trò hiện tại không được phép thực hiện thao tác. Api map sang 403.</summary>
public sealed class ForbiddenTransitionException(string message) : DomainException(message);
