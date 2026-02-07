using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Configuration
// --------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("WARNING: DefaultConnection is not configured");
}

// --------------------------------------------------
// Services
// --------------------------------------------------
builder.Services.AddDbContext<CmsDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseMySQL(connectionString, sql =>
        {
            sql.EnableRetryOnFailure();
        });
    }
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CmsDbContext>(
        name: "mysql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "sql"]
    );

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// --------------------------------------------------
// App
// --------------------------------------------------
var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Multi-Tenant CMS API is running");

app.Run();