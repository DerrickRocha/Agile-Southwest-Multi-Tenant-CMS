using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        using var request = CreateUpdateRequest(null, "missing-header-user", "Admin");
        var response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTenant_Returns403_WhenUserNotMemberOfTenant()
    {
        var (tenantId, userId) = await CreateTenantAndUserAsync(null);
        using var request = CreateUpdateRequest(tenantId, userId, "Admin");
        var response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTenant_Returns401_WhenMembershipIsNotAdmin()
    {
        var (tenantId, userId) = await CreateTenantAndUserAsync(UserTenantRole.Viewer);
        using var request = CreateUpdateRequest(tenantId, userId, "Admin");
        var response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTenant_ReturnsOk_WhenTenantAdmin()
    {
        var (tenantId, userId) = await CreateTenantAndUserAsync(UserTenantRole.Admin);
        using var request = CreateUpdateRequest(tenantId, userId, "Admin");
        var response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
        var updatedTenant = await db.Tenants.SingleAsync(tenant => tenant.Id == tenantId);
        Assert.AreEqual("Updated", updatedTenant.Name);
        Assert.AreEqual("updated", updatedTenant.SubDomain);
    }

    private static HttpRequestMessage CreateUpdateRequest(int? tenantId, string userId, string role)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/tenants")
        {
            Content = JsonContent.Create(new { name = "Updated", subDomain = "updated" })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Test");
        request.Headers.Add("X-Test-User-Id", userId);
        request.Headers.Add("X-Test-Role", role);

        if (tenantId.HasValue)
        {
            request.Headers.Add("X-Tenant-Id", tenantId.Value.ToString());
        }

        return request;
    }

    private async Task<(int TenantId, string UserId)> CreateTenantAndUserAsync(UserTenantRole? membershipRole)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
        var userId = Guid.NewGuid().ToString("N");
        var tenant = new Tenant
        {
            Name = "Test Tenant",
            SubDomain = $"test-{Guid.NewGuid():N}",
            NotificationEmail = "tenant@example.com"
        };
        var user = new CmsUser
        {
            CognitoUserId = userId,
            Email = $"{userId}@example.com",
            Role = UserRole.User,
            Status = UserStatus.Active
        };

        db.Tenants.Add(tenant);
        db.CmsUsers.Add(user);
        if (membershipRole.HasValue)
        {
            db.UserTenants.Add(new UserTenant
            {
                Tenant = tenant,
                User = user,
                Role = membershipRole.Value
            });
        }

        await db.SaveChangesAsync();

        return (tenant.Id, userId);
    }
}