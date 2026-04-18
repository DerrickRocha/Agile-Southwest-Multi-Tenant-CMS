using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Application.DTOs.Stores;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class StoresService(ITenantContext tenantContext, CmsDbContext database): IStoresService
{
    public async Task<AddStoreResult> AddStore(AddStoreRequest request)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var normalizedSubdomain = request.SubDomain.Trim().ToLowerInvariant();

        var store = new Store
        {
            Tenant = tenant,
            Name = request.Name,
            IsOnline = request.IsOnline,
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
        var store = await database.Stores.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenant.Id) ?? throw new InvalidOperationException("Store not found.");
        return new AddStoreResult(store.Id, store.TenantId, store.Name, store.SubDomain, store.IsOnline);
    }
}