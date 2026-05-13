
using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingZoneLocation
{
    public int Id { get; set; }
    public int ShippingZoneId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? RadiusKm { get; set; }
    
    public LocationType Type { get; set; }  // Country, State/Province, PostalCode, Continent
    public string Code { get; set; }  // "US", "CA", "NY", "90210", "North America"
    public string? Name { get; set; }  // Optional display name
}