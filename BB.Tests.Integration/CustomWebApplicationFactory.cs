using BB.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BB.API;
using Core.Common.Logger;
using Core.Common.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace BB.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            // Remove RabbitMQ
            var rabbitDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(RabbitMqOptions));
            if (rabbitDescriptor != null)
                services.Remove(rabbitDescriptor);

            var loggerFactory = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRemoteLoggerFactory));
            if (loggerFactory != null)
                services.Remove(loggerFactory);
        });
    }
}