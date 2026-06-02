using AgileSouthwestCMSAPI.Api.Requests.ShippingRates;
using AgileSouthwestCMSAPI.Application.DTOs.ShippingRates;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ShippingRateService(ITenantContext tenantContext, CmsDbContext database) : IShippingRateService
{
    public async Task<ShippingRateResult> AddShippingRate(AddShippingRateRequest request)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        /*var zone =
            await database.ShippingZones.FirstOrDefaultAsync(z =>
                z.Id == request.ShippingZoneId && z.TenantId == tenant.Id) ??
            throw new InvalidOperationException("Shipping zone not found");
        if (request.PriceCents == null)
        {
            throw new ArgumentException("Price cannot be null");
        }
        var rate = new ShippingRate
        {
            Tenant = tenant,
            ShippingZone = zone,
            RateName = request.RateName,
            MinWeight = request.MinWeight,
            MaxWeight = request.MaxWeight,
            PriceCents = request.PriceCents ?? 0,
        };
        database.ShippingRates.Add(rate);
        await database.SaveChangesAsync();
        return new ShippingRateResult(
            rate.Id,
            rate.TenantId,
            rate.ShippingZoneId
            , rate.RateName,
            rate.MinWeight,
            rate.MaxWeight?? 0,
            rate.PriceCents,
            rate.CreatedAt,
            rate.UpdatedAt,
            rate.DeletedAt
        );*/
        return new ShippingRateResult(0, 0, 0, "", 0, 0, 0, DateTime.UtcNow, DateTime.UtcNow, null);
    }

    public async Task<ShippingRateResult> GetShippingRate(int id)
    {
        /*var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var rate = await database.ShippingRates
            .Where(shippingRate => shippingRate.TenantId == tenant.Id && shippingRate.Id == id).Select(shippingRate =>
                new ShippingRateResult(shippingRate.Id, shippingRate.TenantId, shippingRate.ShippingZoneId,
                    shippingRate.RateName, shippingRate.MinWeight, shippingRate.MaxWeight?? 0, shippingRate.PriceCents,
                    shippingRate.CreatedAt, shippingRate.UpdatedAt, shippingRate.DeletedAt)).FirstOrDefaultAsync();
        return rate ?? throw new InvalidOperationException("Shipping rate not found");*/
        return new ShippingRateResult(0, 0, 0, "", 0, 0, 0, DateTime.UtcNow, DateTime.UtcNow, null);
    }
}