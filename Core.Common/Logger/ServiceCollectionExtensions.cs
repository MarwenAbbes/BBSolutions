using Core.Common.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Common.Logger;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqLogger(
        this IServiceCollection services,
        Action<RabbitMqOptions> configure)
    {
        var options = new RabbitMqOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IRemoteLoggerFactory, RabbitMqLoggerFactory>();

        return services;
    }
}