using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Exceptions;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Domain.ValueObjects;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class TenantsService(CmsDbContext database, ITenantContext context, ICmsUserContext userContext)
    : ITenantsService
{
    public async Task<AddTenantResult> AddTenant(AddTenantRequest request)
    {
        var user = await database.CmsUsers
                       .SingleOrDefaultAsync(u => u.CognitoUserId == userContext.UserId)
                   ?? throw new UnauthorizedAccessException("User not found.");

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
            RowVersion = tenant.RowVersion,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }

    public Task<GetTenantResult> GetTenant()
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        return Task.FromResult(new GetTenantResult
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            CustomDomain = tenant.CustomDomain ?? "",
            SubDomain = tenant.SubDomain,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
            RowVersion = tenant.RowVersion
        });
    }

    public async Task<GetTenantResult[]> GetAllTenantsForAdmin()
    {
        var user = await database.CmsUsers
                       .SingleOrDefaultAsync(u => u.CognitoUserId == userContext.UserId)
                   ?? throw new UnauthorizedAccessException("User not found.");
        var data = database
            .UserTenants
            .AsNoTracking()
            .Where(ut => ut.User.Id == user.Id)
            .Select(ut =>
                new GetTenantResult
                {
                    TenantId = ut.TenantId,
                    Name = ut.Tenant.Name,
                    CustomDomain = ut.Tenant.CustomDomain ?? "",
                    SubDomain = ut.Tenant.SubDomain,
                    CreatedAt = ut.Tenant.CreatedAt,
                    UpdatedAt = ut.Tenant.UpdatedAt,
                    RowVersion = ut.Tenant.RowVersion
                }
            )
            .ToArray();
        
        return data;
    }

    public async Task<UpdateTenantResult> UpdateTenant(UpdateTenantRequest request)
    {
        if (context.Membership?.Role != UserTenantRole.Admin)
            throw new UnauthorizedAccessException("Admin role required.");

        var normalizedSubdomain = request.SubDomain.Trim().ToLowerInvariant();
        var normalizedCustomDomain = request.CustomDomain?.Trim().ToLowerInvariant();

        if (!string.Equals(request.SubDomain, normalizedSubdomain, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await database.Tenants
                .AnyAsync(t => t.SubDomain == normalizedSubdomain && t.Id != request.Id);

            if (exists)
                throw new InvalidOperationException("Subdomain already in use.");
        }

        if (!string.Equals(request.CustomDomain, normalizedCustomDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(normalizedCustomDomain))
            {
                var exists = await database.Tenants
                    .AnyAsync(t => t.CustomDomain == normalizedCustomDomain && t.Id != request.Id);

                if (exists)
                    throw new InvalidOperationException("Custom domain already in use.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Tenant name cannot be empty.", nameof(request));
        }
        var tenant = await database.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id)?? throw new KeyNotFoundException("Tenant not found.");

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
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt,
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
}