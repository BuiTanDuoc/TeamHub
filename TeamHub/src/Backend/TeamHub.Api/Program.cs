using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TeamHub.Api;
using TeamHub.Api.Auth;
using TeamHub.Api.Endpoints;
using TeamHub.Api.Hubs;
using TeamHub.Api.Jobs;
using TeamHub.Api.Middleware;
using TeamHub.Api.OpenApi;
using TeamHub.Application;
using TeamHub.Application.Common;
using TeamHub.Application.Notifications;
using TeamHub.Application.Tasks;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Infrastructure;
using TeamHub.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ---------- Người dùng hiện tại, persistence, identity ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddPersistence(configuration);
builder.Services.AddIdentityServices(configuration);
builder.Services.AddApplication();
builder.Services.AddTeamHubJobs(configuration);
builder.Services.AddSingleton<INotificationPusher, SignalRNotificationPusher>();
builder.Services.Configure<PagingOptions>(configuration.GetSection(PagingOptions.SectionName));
builder.Services.Configure<KanbanOptions>(configuration.GetSection(KanbanOptions.SectionName));

// ---------- Xác thực JWT + phân quyền ----------
builder.Services.AddJwtAuthentication();
builder.Services.AddAuthorizationBuilder()
    // Mặc định MỌI endpoint đều yêu cầu đăng nhập; endpoint công khai phải khai báo .AllowAnonymous()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// ---------- SignalR ----------
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();

// ---------- API: JSON, validation, lỗi, OpenAPI ----------
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidation(); // .NET 10: validate DataAnnotations cho tham số minimal API
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddTeamHubOpenApi();

// ---------- CORS (SignalR JS client gửi credentials khi negotiate nên cần AllowCredentials + origin cụ thể) ----------
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// ---------- Giới hạn tần suất cho endpoint đăng nhập ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    var section = configuration.GetSection("RateLimiting:Auth");
    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = section.GetValue<int>("PermitLimit", 10),
                Window = TimeSpan.FromSeconds(section.GetValue<int>("WindowSeconds", 60)),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// ---------- Pipeline ----------
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();   // /openapi/v1.json
    app.MapSwaggerUi().AllowAnonymous(); // /swagger
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ---------- Endpoints ----------
app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" })).AllowAnonymous();
app.MapAuthEndpoints();
app.MapTeamsEndpoints();
app.MapUsersEndpoints();
app.MapProjectsEndpoints();
app.MapTasksEndpoints();
app.MapCommentsEndpoints();
app.MapNotificationsEndpoints();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<BoardHub>("/hubs/board");

await app.InitializeDatabaseAsync();
app.UseTeamHubRecurringJobs();
app.Run();

// Để WebApplicationFactory<Program> dùng được trong TeamHub.Api.IntegrationTests
public partial class Program;
