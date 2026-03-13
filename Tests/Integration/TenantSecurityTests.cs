using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Integration;

public class TenantSecurityTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task UpdateTenant_Returns400_WhenTenantHeaderMissing()
    {
        var request = new
        {
            name = "Updated",
            subDomain = "updated",
            rowVersion = new byte[] {1}
        };

        var response = await _client.PutAsJsonAsync("/tenants", request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateTenant_Returns403_WhenUserNotMemberOfTenant()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();

        db.Tenants.Add(new Tenant
        {
            Name = "Test Tenant",
            SubDomain = "test"
        });

        await db.SaveChangesAsync();

        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Test");

        var request = new
        {
            name = "Updated",
            subDomain = "updated",
            rowVersion = new byte[] {1}
        };

        var response = await _client.PutAsJsonAsync("/tenants", request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateTenant_Returns403_WhenUserIsNotAdmin()
    {
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "1");

        var request = new
        {
            name = "Updated",
            subDomain = "updated",
            rowVersion = new byte[] {1}
        };

        var response = await _client.PutAsJsonAsync("/tenants", request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}