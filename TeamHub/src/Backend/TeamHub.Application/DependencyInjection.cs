using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using TeamHub.Application.Common.Behaviors;
using TeamHub.Application.Notifications;

namespace TeamHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Source generator của Mediator sinh sẵn AddMediator (không dùng reflection lúc chạy)
        services.AddMediator(options =>
        {
            // Handler/behavior cần IAppDbContext (scoped) nên phải đăng ký Scoped, không dùng Singleton mặc định
            options.ServiceLifetime = ServiceLifetime.Scoped;

            // Thứ tự quan trọng (ngoài → trong): kiểm tra quyền trước, rồi mới validate dữ liệu
            options.PipelineBehaviors = [typeof(AuthorizationBehavior<,>), typeof(ValidationBehavior<,>)];
        });

        // Scoped: gom thông báo của một request, đẩy realtime sau khi lưu
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
