using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamHub.Application.Common.Abstractions;
using TeamHub.Infrastructure.Persistence;
using TeamHub.Infrastructure.Persistence.Interceptors;

namespace TeamHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.EnableRetryOnFailure())
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>())
            // Cảnh báo "required navigation + query filter": các entity con (WorkItem, Bug...) đã có
            // filter team tương đương Project nên an toàn; bỏ cảnh báo để log gọn.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
