using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Exceptions;
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
        var user = await database.CmsUsers.SingleOrDefaultAsync(u => u.CognitoUserId == context.UserId);

        if (user == null) throw new InvalidOperationException("User not found.");
        if (user.Role != UserRole.Admin) throw new UnauthorizedAccessException("User is not authorized to add tenants.");

        var normalizedSubdomain = request.SubDomain.Trim().ToLowerInvariant();
        var normalizedCustomDomain = request.CustomDomain?.Trim().ToLowerInvariant();

        var tenant = new Tenant
        {
            Name = request.Name,
            CustomDomain = normalizedCustomDomain,
            SubDomain = normalizedSubdomain
        };
        database.Tenants.Add(tenant);

        var userTenant = new UserTenant
        {
            User = user,
            Tenant = tenant,
            Role = UserTenantRole.Admin
        };
        database.UserTenants.Add(userTenant);
        
        try
        {
            await database.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Subdomain or custom domain already in use.");
        }
        return new AddTenantResult
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            CustomDomain = tenant.CustomDomain ?? "",
            SubDomain = tenant.SubDomain,
            RowVersion = tenant.RowVersion
        };
    }

    public async Task<GetTenantResult> GetTenant(GetTenantRequest request)
    {
        var tenant = await GetAuthorizedTenant(request.Id) ??
                     throw new UnauthorizedAccessException("Tenant not found or unauthorized.");
        return new GetTenantResult
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            CustomDomain = tenant.CustomDomain ?? "",
            SubDomain = tenant.SubDomain,
            RowVersion = tenant.RowVersion
        };
    }

    public async Task<UpdateTenantResult> UpdateTenant(UpdateTenantRequest request)
    {
        var tenant = await GetAuthorizedTenant(request.Id) ??
                     throw new UnauthorizedAccessException("Tenant not found or unauthorized.");

        var normalizedSubdomain = request.SubDomain.Trim().ToLowerInvariant();
        var normalizedCustomDomain = request.CustomDomain?.Trim().ToLowerInvariant();

        if (!string.Equals(tenant.SubDomain, normalizedSubdomain, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await database.Tenants
                .AnyAsync(t => t.SubDomain == normalizedSubdomain && t.Id != tenant.Id);

            if (exists)
                throw new InvalidOperationException("Subdomain already in use.");
        }

        if (!string.Equals(tenant.CustomDomain, normalizedCustomDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(normalizedCustomDomain))
            {
                var exists = await database.Tenants
                    .AnyAsync(t => t.CustomDomain == normalizedCustomDomain && t.Id != tenant.Id);

                if (exists)
                    throw new InvalidOperationException("Custom domain already in use.");
            }
        }

        tenant.Name = request.Name;
        tenant.SubDomain = normalizedSubdomain;
        tenant.CustomDomain = normalizedCustomDomain;

        database.Entry(tenant)
            .Property(t => t.RowVersion)
            .OriginalValue = request.RowVersion;

        try
        {
            await database.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(
                "This tenant was modified by another user. Please refresh and try again.");
        }

        return new UpdateTenantResult
        {
            Id = tenant.Id,
            SubDomain = tenant.SubDomain,
            CustomDomain = tenant.CustomDomain ?? "",
            Name = tenant.Name,
            RowVersion = tenant.RowVersion
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

    private Task<Tenant?> GetAuthorizedTenant(int tenantId)
    {
        return database.Tenants
            .Where(t => t.Id == tenantId)
            .Where(t => t.UserTenants
                .Any(ut => ut.User.CognitoUserId == context.UserId && ut.Role == UserTenantRole.Admin))
            .SingleOrDefaultAsync();
    }
}