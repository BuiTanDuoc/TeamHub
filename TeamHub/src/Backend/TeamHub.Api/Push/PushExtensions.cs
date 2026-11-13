using Microsoft.Extensions.Options;
using TeamHub.Application.Notifications;

namespace TeamHub.Api.Push;

public static class PushExtensions
{
    public static IServiceCollection AddTeamHubPush(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExpoPushOptions>(configuration.GetSection(ExpoPushOptions.SectionName));

        if (!configuration.GetValue("Push:Expo:Enabled", true))
            return services.AddSingleton<IPushSender, NoOpPushSender>();

        services.AddHttpClient<IPushSender, ExpoPushSender>((sp, client) =>
            client.Timeout = TimeSpan.FromSeconds(sp.GetRequiredService<IOptions<ExpoPushOptions>>().Value.TimeoutSeconds));

        return services;
    }
}
