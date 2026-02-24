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

            var user = await database.CmsUsers.SingleOrDefaultAsync(u => u.CognitoUserId == context.UserId);

            if (user == null) throw new InvalidOperationException("User not found.");
            if (user.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("User is not authorized to add tenants.");

            var subdomainExists = await database.Tenants
                .AnyAsync(t => t.SubDomain == request.SubDomain);
            if (subdomainExists) throw new InvalidOperationException("Subdomain already in use.");

            if (request.CustomDomain != null)
            {
                var customDomainExists = await database.Tenants.AnyAsync(t => t.CustomDomain == request.CustomDomain);
                if (customDomainExists) throw new InvalidOperationException("Custom domain already in user.");
            }

            var tenant = new Tenant
            {
                Name = request.Name,
                CustomDomain = request.CustomDomain,
                SubDomain = request.SubDomain
            };
            database.Tenants.Add(tenant);

            var userTenant = new UserTenant
            {
                User = user,
                Tenant = tenant,
                Role = UserTenantRole.Admin
            };
            database.UserTenants.Add(userTenant);
            await database.SaveChangesAsync();
            await transaction.CommitAsync();

            return new AddTenantResult
            {
                TenantId = tenant.Id,
                Name = tenant.Name,
                CustomDomain = tenant.CustomDomain ?? "",
                SubDomain = tenant.SubDomain
            };
        });
    }

    public async Task<GetTenantResult> GetTenant(GetTenantRequest request)
    {
        var tenant = await database.Tenants
            .Where(t => t.Id == request.Id)
            .Where(t => t.UserTenants.Any(ut => ut.User.CognitoUserId == context.UserId))
            .Where(t => t.UserTenants.Any(ut => ut.Role == UserTenantRole.Admin))
            .SingleOrDefaultAsync();
        if (tenant == null) throw new UnauthorizedAccessException("Tenant not found or unauthorized.");
        
        return new GetTenantResult
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            CustomDomain = tenant.CustomDomain ?? "",
            SubDomain = tenant.SubDomain,
        };
    }

    public async Task<UpdateTenantResult> UpdateTenant(UpdateTenantRequest request)
    {
        var tenant = await database.Tenants
            .Where(t => t.Id == request.Id)
            .Where(t => t.UserTenants.Any(ut => ut.User.CognitoUserId == context.UserId))
            .Where(t => t.UserTenants.Any(ut => ut.Role == UserTenantRole.Admin))
            .SingleOrDefaultAsync();

        if (tenant == null)
            throw new UnauthorizedAccessException("Tenant not found or unauthorized.");
        
        var normalizedSubdomain = request.SubDomain.Trim().ToLowerInvariant();
        var normalizedCustomDomain = request.CustomDomain?.Trim().ToLowerInvariant();

        if (!string.Equals(tenant.SubDomain, normalizedSubdomain, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await database.Tenants
                .AnyAsync(t => t.SubDomain == request.SubDomain && t.Id != tenant.Id);

            if (exists)
                throw new InvalidOperationException("Subdomain already in use.");
        }

        if (!string.Equals(tenant.CustomDomain, normalizedCustomDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(request.CustomDomain))
            {
                var exists = await database.Tenants
                    .AnyAsync(t => t.CustomDomain == request.CustomDomain && t.Id != tenant.Id);

                if (exists)
                    throw new InvalidOperationException("Custom domain already in use.");
            }
        }
        
        tenant.Name = request.Name;
        tenant.SubDomain = request.SubDomain;
        tenant.CustomDomain = request.CustomDomain;
        
        await database.SaveChangesAsync();
        
        return new UpdateTenantResult
        {
            Id = tenant.Id,
            SubDomain = tenant.SubDomain,
            CustomDomain = tenant.CustomDomain ?? "",
            Name = tenant.Name
        };
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