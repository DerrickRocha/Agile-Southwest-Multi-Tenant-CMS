using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Application.DTOs.Stores;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class StoresService(ITenantContext tenantContext, CmsDbContext database) : IStoresService
{
    public async Task<AddStoreResult> AddStore(AddStoreRequest request)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var normalizedSubdomain = request.SubDomain.Trim().ToLowerInvariant();

        var isOnline = request.IsOnline ?? throw new InvalidOperationException("isOnline must not be null.");

        var exists = await database.Stores.FirstOrDefaultAsync(s =>
            s.TenantId == tenant.Id && s.SubDomain == request.SubDomain && s.DeletedAt == null);

        if (exists != null)
        {
            throw new InvalidOperationException(
                $"A store with subdomain '{request.SubDomain}' already exists");
        }

// Check for soft-deleted store that could be restored
        var deletedStore = await database.Stores
            .IgnoreQueryFilters() // Bypass soft-delete filter if you have one
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenant.Id &&
                s.SubDomain == request.SubDomain &&
                s.DeletedAt != null);

        if (deletedStore != null)
        {
            throw new InvalidOperationException(
                $"A store with subdomain '{request.SubDomain}' was previously deleted. " +
                "Please restore it instead of creating a new one.");
        }

        var store = new Store
        {
            Tenant = tenant,
            Name = request.Name,
            IsOnline = isOnline,
            SubDomain = normalizedSubdomain
        };

        database.Stores.Add(store);
        try
        {
            await database.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Subdomain already in use.");
        }

        return new AddStoreResult(store.Id, store.TenantId, store.Name, store.SubDomain, store.IsOnline);
    }

    public async Task<AddStoreResult> GetStore(int id)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var store = await database.Stores.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenant.Id) ??
                    throw new InvalidOperationException("Store not found.");
        return new AddStoreResult(store.Id, store.TenantId, store.Name, store.SubDomain, store.IsOnline);
    }
}