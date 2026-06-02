using AgileSouthwestCMSAPI.Api.Requests.ZonePostalCodes;
using AgileSouthwestCMSAPI.Application.DTOs.ZonePostalCodes;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ZonePostalCodesService(ITenantContext tenantContext, CmsDbContext database) : IZonePostalCodeService
{
    public async Task<ZonePostalCodeResult> AddZonePostalCode(AddZonePostalCodeRequest request)
    {
        /*var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var zone =
            await database.ShippingZones.FirstOrDefaultAsync(z =>
                z.Id == request.ShippingZoneId && z.TenantId == tenant.Id) ??
            throw new InvalidOperationException("Shipping zone not found");
        var code = new ZonePostalCode
        {
            Tenant = tenant,
            ShippingZone = zone,
            PostalCode = request.PostalCode
        };
        database.ZonePostalCodes.Add(code);
        await database.SaveChangesAsync();
        return new ZonePostalCodeResult(code.Id, code.ShippingZoneId, code.PostalCode, code.CreatedAt, code.UpdatedAt,
            code.DeletedAt, code.RowVersion);*/
        return new ZonePostalCodeResult(0, 0, "", DateTime.UtcNow, DateTime.UtcNow, null, new DateTime());
    }

    public async Task<ZonePostalCodeResult> GetZonePostalCode(int id)
    {
       /* var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var code = await database.ZonePostalCodes.Where(z => z.Id == id && z.TenantId == tenant.Id).Select(z =>
                       new ZonePostalCodeResult(z.Id, z.ShippingZoneId, z.PostalCode, z.CreatedAt, z.UpdatedAt,
                           z.DeletedAt,
                           z.RowVersion)).FirstOrDefaultAsync() ??
                   throw new InvalidOperationException("Zone postal code not found");
        return code;*/
       return new ZonePostalCodeResult(0, 0, "", DateTime.UtcNow, DateTime.UtcNow, null, new DateTime());
    }
}