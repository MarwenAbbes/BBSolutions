using BB.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BB.API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BB.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        // Program.cs reads Jwt:Secret directly from builder.Configuration BEFORE
        // builder.Build() runs, which means ConfigureAppConfiguration callbacks
        // registered via the factory aren't applied yet. Environment variables,
        // however, are loaded by WebApplication.CreateBuilder up-front, so set
        // them here to satisfy the startup validation.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Jwt__Secret", "your-super-secret-key-that-is-at-least-32-characters");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "BB.API");
        Environment.SetEnvironmentVariable("Jwt__Audience", "BB.API");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
    }
}