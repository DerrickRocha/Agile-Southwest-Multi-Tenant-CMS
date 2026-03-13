using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test"); // Important

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations (if any)
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<CmsDbContext>) ||
                            d.ServiceType == typeof(CmsDbContext))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            // Add InMemory for testing
            services.AddDbContext<CmsDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())
            );

            // Fake authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }
}