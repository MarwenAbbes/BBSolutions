    using System.Text;
    using Asp.Versioning;
    using BB.Domain.Interfaces;
    using BB.Infrastructure.Data;
    using BB.Infrastructure.Repositories;
    using BB.Infrastructure.Security;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;

var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

    builder.Services.AddProblemDetails();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();


    public partial class Program { }