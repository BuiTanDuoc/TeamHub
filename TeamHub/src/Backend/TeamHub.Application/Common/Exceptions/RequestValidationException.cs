namespace TeamHub.Application.Common.Exceptions;

/// <summary>Dữ liệu đầu vào không hợp lệ (từ FluentValidation). Api map sang 400 kèm lỗi theo field.</summary>
public sealed class RequestValidationException(IDictionary<string, string[]> errors)
    : Exception("Dữ liệu không hợp lệ.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
