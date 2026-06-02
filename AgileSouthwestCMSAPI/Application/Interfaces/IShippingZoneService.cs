using AgileSouthwestCMSAPI.Api.Requests.ShippingZones;
using AgileSouthwestCMSAPI.Application.DTOs.ShippingZones;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IShippingZoneService
{
    public Task<ShippingZoneResult> AddShippingZone(ShippingZoneRequest request);
    
    public Task<ShippingZoneResult> GetShippingZone(int id);
}