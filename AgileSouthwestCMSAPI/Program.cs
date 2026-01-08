using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// REQUIRED: Elastic Beanstalk port binding
// --------------------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// --------------------------------------------------
// Configuration
// --------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DO NOT crash the app at startup in EB
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
        options.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure();
        });
    }
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CmsDbContext>(
        name: "sqlserver",
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

// ❌ REMOVE HTTPS REDIRECTION FOR EB
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// REQUIRED for EB health checks
app.MapHealthChecks("/health");

// Simple root endpoint for verification
app.MapGet("/", () => "Multi-Tenant CMS API is running");

app.Run();