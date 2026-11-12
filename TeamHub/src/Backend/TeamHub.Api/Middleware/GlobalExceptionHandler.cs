using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamHub.Application.Common.Exceptions;
using TeamHub.Domain.Exceptions;

namespace TeamHub.Api.Middleware;

/// <summary>Chuyển exception nghiệp vụ thành ProblemDetails (RFC 9457) với mã HTTP phù hợp.</summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (Map(exception) is not { } mapped)
        {
            // Lỗi không lường trước: ghi log, để framework trả 500 ProblemDetails (không lộ chi tiết ở Production)
            logger.LogError(exception, "Lỗi không xử lý được: {Path}", httpContext.Request.Path);
            return false;
        }

        var (status, title) = mapped;
        httpContext.Response.StatusCode = status;

        var detail = exception is DbUpdateConcurrencyException
            ? "Vui lòng tải lại dữ liệu và thử lại."
            : exception.Message;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }

    // Thứ tự quan trọng: các lớp con của DomainException phải đứng trước DomainException
    private static (int Status, string Title)? Map(Exception exception) => exception switch
    {
        AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "Xác thực thất bại"),
        ForbiddenException or ForbiddenTransitionException => (StatusCodes.Status403Forbidden, "Không có quyền thực hiện"),
        NotFoundException => (StatusCodes.Status404NotFound, "Không tìm thấy"),
        InvalidTransitionException => (StatusCodes.Status409Conflict, "Chuyển trạng thái không hợp lệ"),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Dữ liệu đã bị người khác thay đổi"),
        DomainException => (StatusCodes.Status400BadRequest, "Vi phạm quy tắc nghiệp vụ"),
        _ => null
    };
}
