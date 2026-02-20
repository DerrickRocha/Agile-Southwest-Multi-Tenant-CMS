using System.Security.Claims;
using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class TenantsService(CmsDbContext database, IHttpContextAccessor accessor) : ITenantsService
{
    public async Task<GetTenantResult> GetTenant()
    {
        var httpContext = accessor.HttpContext 
                          ?? throw new InvalidOperationException("No active HTTP context");
        var email = httpContext.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email)) throw new InvalidEmailRoleAccessPolicyException("Email is invalid");

        var user = await database.CmsUsers.FirstOrDefaultAsync(u => u.Email == email)?? throw new InvalidOperationException("User not found");
        var tenant = await database.Tenants.FirstOrDefaultAsync(t => t.TenantId == user.TenantId)?? throw new InvalidOperationException("Tenant not found");
        return new GetTenantResult
        {
            TenantId = tenant.TenantId.ToString()
        };
    }

    public Task<UpdateTenantResult> UpdateTenant(UpdateTenantRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<GetTenantSubscriptionResult> GetTenantSubscription(GetTenantSubscriptionRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ChangeTenantSubscriptionResult> UpdateTenantSubscription(ChangeTenantSubsciptionRequest request)
    {
        throw new NotImplementedException();
    }
}