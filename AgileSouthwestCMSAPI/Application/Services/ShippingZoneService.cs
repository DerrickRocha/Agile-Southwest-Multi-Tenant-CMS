using AgileSouthwestCMSAPI.Api.Requests.ShippingZones;
using AgileSouthwestCMSAPI.Application.DTOs.ShippingZones;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ShippingZoneService(CmsDbContext dbContext, ITenantContext tenantContext) : IShippingZoneService
{
    public async Task<ShippingZoneResult> AddShippingZone(ShippingZoneRequest request)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var zone = new ShippingZone
        {
            Tenant = tenant,
            Name = request.Name,
            IsLocalFleet = request.IsLocalFleet,
        };

        dbContext.ShippingZones.Add(zone);
        await dbContext.SaveChangesAsync();
        return new ShippingZoneResult(zone.Id, zone.Name, zone.IsLocalFleet, zone.CreatedAt, zone.UpdatedAt);
    }

    public async Task<ShippingZoneResult> GetShippingZone(int id)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");

        var zone = await dbContext.ShippingZones
            .AsNoTracking()
            .Where(z => z.Id == id && z.TenantId == tenant.Id)
            .Select(z => new ShippingZoneResult(
                    z.Id,
                    z.Name,
                    z.IsLocalFleet,
                    z.CreatedAt,
                    z.UpdatedAt
                )
            )
            .FirstOrDefaultAsync();
        
        return zone ?? throw new InvalidOperationException("Shipping zone not found");
    }
}