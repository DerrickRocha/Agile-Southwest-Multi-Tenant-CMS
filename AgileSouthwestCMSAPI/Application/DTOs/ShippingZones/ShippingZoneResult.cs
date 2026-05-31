namespace AgileSouthwestCMSAPI.Application.DTOs.ShippingZones;

public record ShippingZoneResult(int Id, string Name, bool IsLocalFleet, DateTime CreatedAt, DateTime UpdatedAt);