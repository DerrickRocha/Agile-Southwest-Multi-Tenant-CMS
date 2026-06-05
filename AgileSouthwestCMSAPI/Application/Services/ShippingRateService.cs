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
        if (request.PriceCents == null)
        {
            throw new ArgumentException("Price cannot be null");
        }
        var rate = new ShippingRate
        {
            Tenant = tenant,
            RateName = request.RateName,
            MinWeightGrams = request.MinWeight,
            MaxWeightGrams = request.MaxWeight,
            PriceCents = request.PriceCents ?? 0,
            PostalCode = request.PostalCode ?? throw new ArgumentException("Postal code cannot be null")
        };
        database.ShippingRates.Add(rate);
        await database.SaveChangesAsync();
        return new ShippingRateResult(
            rate.Id,
            rate.TenantId,
            rate.RateName,
            rate.MinWeightGrams,
            rate.MaxWeightGrams??0,
            rate.PriceCents,
            rate.CreatedAt,
            rate.UpdatedAt,
            rate.DeletedAt
        );
    }

    public async Task<ShippingRateResult> GetShippingRate(int id)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var rate = await database.ShippingRates
            .Where(shippingRate => shippingRate.TenantId == tenant.Id && shippingRate.Id == id).Select(shippingRate =>
                new ShippingRateResult(shippingRate.Id, shippingRate.TenantId,
                    shippingRate.RateName, shippingRate.MinWeightGrams, shippingRate.MaxWeightGrams?? 0, shippingRate.PriceCents,
                    shippingRate.CreatedAt, shippingRate.UpdatedAt, shippingRate.DeletedAt)).FirstOrDefaultAsync();
        return rate ?? throw new InvalidOperationException("Shipping rate not found");
    }
}