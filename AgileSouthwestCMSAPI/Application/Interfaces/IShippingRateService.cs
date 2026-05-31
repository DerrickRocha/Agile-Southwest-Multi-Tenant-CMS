using AgileSouthwestCMSAPI.Api.Requests.ShippingRates;
using AgileSouthwestCMSAPI.Application.DTOs.ShippingRates;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IShippingRateService
{
    public Task<ShippingRateResult> AddShippingRate(AddShippingRateRequest request);
    
    public Task<ShippingRateResult> GetShippingRate(int id);
}