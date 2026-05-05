using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using BB.Domain.Interfaces;
using BB.Infrastructure.Data;
using BB.Infrastructure.Repositories;
using BB.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "BBSolutions.API");

    var rabbitHost = context.Configuration["RabbitMq:Host"];
    if (!string.IsNullOrEmpty(rabbitHost))
    {
        config.WriteTo.RabbitMQ((rabbitSink, format) =>
        {
            rabbitSink.Hostnames = [rabbitHost];
            rabbitSink.Port = context.Configuration.GetValue("RabbitMq:Port", 5672);
            rabbitSink.Username = context.Configuration["RabbitMq:Username"] ?? "guest";
            rabbitSink.Password = context.Configuration["RabbitMq:Password"] ?? "guest";
            rabbitSink.Exchange = context.Configuration["RabbitMq:Exchange"] ?? "logs.exchange";
            rabbitSink.ExchangeType = "topic";
            rabbitSink.RoutingKey = "logs.bbsolutions";
        });
    }
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException(
        "Jwt:Secret is not configured. Set it via user-secrets (dev) or environment variable (production): " +
        "dotnet user-secrets set \"Jwt:Secret\" \"<your-key-at-least-32-chars>\" --project BB.API");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

var rabbitHealthHost = builder.Configuration["RabbitMq:Host"];
if (!string.IsNullOrEmpty(rabbitHealthHost))
{
    builder.Services.AddSingleton<IConnection>(sp =>
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitHealthHost,
            Port = builder.Configuration.GetValue("RabbitMq:Port", 5672),
            UserName = builder.Configuration["RabbitMq:Username"] ?? "guest",
            Password = builder.Configuration["RabbitMq:Password"] ?? "guest"
        };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    });
}

builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy)
    .AddRabbitMQ(
        name: "rabbitmq",
        failureStatus: HealthStatus.Degraded);

builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    // Parallel integration tests issue many login requests; a tight prod limit causes flaky 429s in CI.
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.AddPolicy("auth", _ => RateLimitPartition.GetNoLimiter(string.Empty));
    }
    else
    {
        options.AddFixedWindowLimiter("auth", limiter =>
        {
            limiter.PermitLimit = 5;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueLimit = 0;
        });
    }

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds + "ms"
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.Run();

public partial class Program { }
