using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TestAPI.Application.Middleware;
using TestAPI.Domain;
using TestAPI.Domain.Auth.Interfaces;
using TestAPI.Domain.Auth.Services;
using TestAPI.Domain.DataExample.Interfaces;
using TestAPI.Domain.DataExample.Services;
using TestAPI.Infrastructure;
using TestAPI.Infrastructure.Interfaces;
using TestAPI.Infrastructure.Repositories;
using TestAPI.Infrastructure.Repositories.SqlServer;
using TestAPI.Infrastructure.WebSockets.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

#region Service Registration

// Add services to the container.
builder.Services.AddControllers();

// 🔥 Configure CORS - Allow access from any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()       // Allow requests from any origin
              .AllowAnyMethod()       // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader();      // Allow any headers
    });

});

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, ServiceLifetime.Scoped);

// Register Unit of Work (manages transactions and repositories)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register repositories (Infrastructure layer)
builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 🔥 Register WebSocket services as Singleton (required for middleware)
builder.Services.AddSingleton<TestAPI.Infrastructure.WebSockets.Interfaces.IWebSocketManager, TestAPI.Infrastructure.WebSockets.Services.WebSocketManager>();
builder.Services.AddSingleton<TestAPI.Infrastructure.WebSockets.Interfaces.IWebSocketService, TestAPI.Infrastructure.WebSockets.Services.WebSocketService>();

// Register domain services (Domain layer)
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure for ILogger
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

#endregion

#region JWT Authentication Configuration

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove delay of token expiration
    };
});

builder.Services.AddAuthorization();

#endregion

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

#region Middleware Pipeline

// Register Global Exception Handling Middleware (MUST BE FIRST)
app.UseMiddleware<ExceptionMiddleware>();

// 🔥 Enable WebSocket support
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

// 🔥 Register WebSocket middleware
app.UseWebSocketMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🔥 Enable CORS - Must be after UseHttpsRedirection and before UseAuthentication
app.UseCors("AllowAll");

// Authentication & Authorization (ORDER MATTERS)
app.UseAuthentication(); // Must be before UseAuthorization

// 🔥 Populate RequestContext after authentication
app.UseRequestContext(); // Must be after UseAuthentication

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
