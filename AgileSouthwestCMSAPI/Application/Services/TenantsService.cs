using System.Security.Claims;
using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.ValueObjects;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class TenantsService(CmsDbContext database, ICmsUserContext context) : ITenantsService
{
    public async Task<GetTenantResult> GetTenant(GetTenantRequest request)
    {
        if (!Guid.TryParse(request.Id, out var tenantId))
            throw new ArgumentException("Invalid tenant id");
        if (!Guid.TryParse(context.UserId, out var userId))
            throw new UnauthorizedAccessException("Invalid user id");

        var userTenant = await database.UserTenants
                             .AsNoTracking().Include(userTenant => userTenant.Tenant)
                             .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.UserId == userId) ??
                         throw new InvalidOperationException("Tenant not found");
        var tenant = userTenant.Tenant;
        return new GetTenantResult
        {
            TenantId = tenant.TenantId.ToString(),
            Name = tenant.Name,
            CustomDomain = tenant.CustomDomain,
            SubDomain = tenant.SubDomain,
            PlanTier = tenant.PlanTier.ToString()
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