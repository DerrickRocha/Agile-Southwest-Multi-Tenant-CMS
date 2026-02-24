using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Domain.ValueObjects;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class TenantsService(CmsDbContext database, ICmsUserContext context) : ITenantsService
{
    public async Task<AddTenantResult> AddTenant(AddTenantRequest request)
    {
        var strategy = database.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.Database.BeginTransactionAsync();
            
            //1. verify user is admin
            var user = await database.CmsUsers.SingleOrDefaultAsync(u => u.CognitoUserId == context.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }
            if (user.Role != UserRole.Admin)
            {
               throw new UnauthorizedAccessException("User is not authorized to add tenants."); 
            }
            
            var subdomainExists = await database.Tenants
                .AnyAsync(t => t.SubDomain == request.SubDomain);
            if (subdomainExists) throw new InvalidOperationException("Subdomain already in use.");

            if (request.CustomDomain != null)
            {
                var customDomainExists = await database.Tenants.AnyAsync(t => t.CustomDomain == request.CustomDomain);
                if (customDomainExists)
                {
                    throw new InvalidOperationException("Custom domain already in user.");
                }
            }
            
            var tenant = new Tenant
            {
                Name = request.Name,
                CustomDomain = request.CustomDomain,
                SubDomain = request.SubDomain,
                PlanTier = PlanTier.Free,
                SubscriptionStatus = SubscriptionStatus.Active,
                Status = TenantStatus.Active,
            };
            database.Tenants.Add(tenant);

            var userTenant = new UserTenant
            {
                User = user,
                Tenant = tenant,
                Role = UserRole.Admin
            };
            database.UserTenants.Add(userTenant);
            await database.SaveChangesAsync();
            await transaction.CommitAsync();
                
            return new AddTenantResult
            {
                TenantId = tenant.Id,
                Name = tenant.Name,
                CustomDomain = tenant.CustomDomain?? "",
                SubDomain = tenant.SubDomain,
                PlanTier = tenant.PlanTier.ToString(),
                Status = tenant.Status.ToString(),
                SubscriptionStatus = tenant.SubscriptionStatus.ToString()
            };
        });
    }

    public async Task<GetTenantResult> GetTenant(GetTenantRequest request)
    {
        try
        {
            if (request.Id <= 0)
                throw new ArgumentException("Invalid tenant id");
            if (string.IsNullOrEmpty(context.UserId))
                throw new UnauthorizedAccessException("Invalid user id");

            var userTenant = await database.UserTenants
                                 .AsNoTracking()
                                 .Include(ut => ut.User)
                                 .Include(ut => ut.Tenant)
                                 .FirstOrDefaultAsync(ut => ut.TenantId == request.Id) ??
                             throw new InvalidOperationException("UserTenant not found");
            var tenant = userTenant.Tenant;
            return new GetTenantResult
            {
                TenantId = tenant.Id,
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