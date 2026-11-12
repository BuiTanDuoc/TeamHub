using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeamHub.Api.OpenApi;

public static class OpenApiExtensions
{
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// Đăng ký OpenAPI (built-in của .NET 10): thông tin tài liệu, khai báo JWT Bearer
    /// và gắn yêu cầu xác thực cho từng operation để nút "Authorize" trên Swagger UI hoạt động.
    /// </summary>
    public static IServiceCollection AddTeamHubOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "TeamHub API",
                    Version = "v1",
                    Description = "API quản lý software team: team, dự án, công việc, bug và báo cáo. " +
                                  "Đăng nhập qua /api/v1/auth/login rồi bấm Authorize và dán access token (không cần tiền tố \"Bearer\"). " +
                                  "Các hub SignalR (/hubs/notifications, /hubs/board) không nằm trong tài liệu OpenAPI."
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT access token lấy từ /api/v1/auth/login hoặc /api/v1/auth/google."
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, _) =>
            {
                // Fallback policy bắt đăng nhập mọi endpoint, nên chỉ endpoint có .AllowAnonymous() mới không cần token
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                if (metadata.OfType<IAllowAnonymous>().Any())
                    return Task.CompletedTask;

                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = []
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>Trang Swagger UI (chỉ dùng ở Development), đọc tài liệu từ /openapi/v1.json.</summary>
    public static IEndpointConventionBuilder MapSwaggerUi(this IEndpointRouteBuilder app) =>
        app.MapGet("/swagger", () => Results.Content(SwaggerUiHtml, "text/html; charset=utf-8"))
            .ExcludeFromDescription();

    // Tải swagger-ui-dist từ CDN để không phải thêm package NuGet; ghim phiên bản cụ thể
    private const string SwaggerUiHtml = """
        <!DOCTYPE html>
        <html lang="vi">
        <head>
          <meta charset="utf-8" />
          <title>TeamHub API</title>
          <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.17.14/swagger-ui.css" />
        </head>
        <body>
          <div id="swagger-ui"></div>
          <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.17.14/swagger-ui-bundle.js"></script>
          <script>
            window.ui = SwaggerUIBundle({
              url: "/openapi/v1.json",
              dom_id: "#swagger-ui",
              persistAuthorization: true
            });
          </script>
        </body>
        </html>
        """;
}
