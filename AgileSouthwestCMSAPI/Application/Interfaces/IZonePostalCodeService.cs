using AgileSouthwestCMSAPI.Api.Requests.ZonePostalCodes;
using AgileSouthwestCMSAPI.Application.DTOs.ZonePostalCodes;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IZonePostalCodeService
{
    public Task<ZonePostalCodeResult> AddZonePostalCode(AddZonePostalCodeRequest request);
    
    public Task<ZonePostalCodeResult> GetZonePostalCode(int id);
}
