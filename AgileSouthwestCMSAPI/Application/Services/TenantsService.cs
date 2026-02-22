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
        try
        {
            if (!Guid.TryParse(request.Id, out var tenantId))
                throw new ArgumentException("Invalid tenant id");
            if (string.IsNullOrEmpty(context.UserId))
                throw new UnauthorizedAccessException("Invalid user id");

            var userTenant = await database.UserTenants
                                 .AsNoTracking().Include(userTenant => userTenant.Tenant)
                                 .Include(userTenant => userTenant.User)
                                 .FirstOrDefaultAsync() ??
                             throw new InvalidOperationException("UserTenant not found");
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
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