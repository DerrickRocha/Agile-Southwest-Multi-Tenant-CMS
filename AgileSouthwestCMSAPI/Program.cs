using System.Text.Json;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using AgileSouthwestCMSAPI.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Configuration
// --------------------------------------------------


// Load database connectionString
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DefaultConnection is not configured");
    
}

// --------------------------------------------------
// Services
// --------------------------------------------------

// Logging (Serilog / OpenTelemetry-friendly)
builder.Services.AddLogging();

builder.Services.AddDbContext<CmsDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseMySQL(connectionString, sql => { sql.EnableRetryOnFailure(); });
    }
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CmsDbContext>(
        name: "mysql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "sql"]
    );

// Controllers + validation
builder.Services.AddControllers(options =>
        options.Filters.Add<ApiExceptionFilter>()
    )
    .AddJsonOptions(opts => { opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy
            .WithOrigins("https://agilesouthwest.com")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddAuthentication();

builder.Services.AddAuthorization();

// Rate limiting (built-in)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.Window = TimeSpan.FromSeconds(10);
        limiter.PermitLimit = 100;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Compression
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes =
    [
        "application/json"
    ];
});

// Custom middleware DI
builder.Services.AddScoped<RequestLoggingMiddleware>();

// --------------------------------------------------
// App
// --------------------------------------------------
var app = builder.Build();

app.UseForwardedHeaders();

app.UseApiExceptionHandling();

app.UseHttpsRedirection();

app.UseRouting();

// 🔒 Security headers AFTER routing, BEFORE endpoints
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    context.Response.Headers.TryAdd(
        "Content-Security-Policy",
        "default-src 'none'; frame-ancestors 'none';"
    );

    await next();
});

app.UseCors("Default");

app.UseRateLimiter();

app.UseMiddleware<IpAllowListMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health")
    .WithMetadata(new AllowAnonymousAttribute());

app.Run();