    using System.Text;
    using Asp.Versioning;
    using BB.API.Logging;
    using BB.Infrastructure.Data;
    using BB.Infrastructure.Repositories;
    using BB.Infrastructure.Security;
    using Core.Common.Logger;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

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
    builder.Services.AddRabbitMqLogger(options =>
    {
        options.Host     = "127.0.0.1";
        options.Port     = 5672;
        options.Username = "guest";
        options.Password = "guest";
        options.Exchange   = "logs.exchange";
        options.QueueName = "logs";
        options.RoutingKey = "logs.#";
        options.SourceApplication = "BBSolutions.API";
    });
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });
    builder.Logging.AddProvider(new RabbitMqLoggerProvider(
        builder.Services.BuildServiceProvider().GetRequiredService<IRemoteLoggerFactory>()
    ));
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    
    // Configure JWT Authentication
    var jwtSecret = builder.Configuration["Jwt:Secret"]!;
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
    

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();


    public partial class Program { }