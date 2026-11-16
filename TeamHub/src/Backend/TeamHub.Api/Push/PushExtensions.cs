using Microsoft.Extensions.Options;
using TeamHub.Application.Notifications;

namespace TeamHub.Api.Push;

public static class PushExtensions
{
    public static IServiceCollection AddTeamHubPush(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExpoPushOptions>(configuration.GetSection(ExpoPushOptions.SectionName));
        services.Configure<FcmPushOptions>(configuration.GetSection(FcmPushOptions.SectionName));

        services.AddHttpClient<ExpoPushSender>((sp, client) =>
            client.Timeout = TimeSpan.FromSeconds(sp.GetRequiredService<IOptions<ExpoPushOptions>>().Value.TimeoutSeconds));

        // Singleton: giữ FirebaseApp/FirebaseMessaging đã khởi tạo, không tạo lại mỗi request
        services.AddSingleton<FcmPushSender>();

        // Dispatcher chỉ biết IPushSender; việc chọn kênh theo token và theo cờ Enabled nằm ở PushRouter
        services.AddTransient<IPushSender, PushRouter>();

        return services;
    }
}
