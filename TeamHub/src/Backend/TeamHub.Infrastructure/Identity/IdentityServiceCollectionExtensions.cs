using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Domain.Users;
using TeamHub.Infrastructure.Persistence;

namespace TeamHub.Infrastructure.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart(); // thiếu Jwt:SigningKey thì app không khởi động

        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        // Chỉ dùng UserManager (không cần cookie/SignInManager vì xác thực bằng JWT)
        services.AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Chính sách mật khẩu, khóa tài khoản, email duy nhất: đọc từ appsettings.json mục "Identity"
        services.Configure<IdentityOptions>(configuration.GetSection("Identity"));

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
