using System.Text.Json;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using AgileSouthwestCMSAPI.Middleware;
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
    Console.WriteLine("WARNING: DefaultConnection is not configured");
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

// Auth
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// --------------------------------------------------
// App
// --------------------------------------------------
var app = builder.Build();

app.UseApiExceptionHandling();
app.UseCors("Default");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Multi-Tenant CMS API is running");

app.Run();