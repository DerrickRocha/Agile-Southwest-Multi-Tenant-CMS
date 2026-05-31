namespace AgileSouthwestCMSAPI.Application.DTOs.ZonePostalCodes;

public record ZonePostalCodeResult(
    int Id,
    int ShippingZoneId,
    string PostalCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt,
    DateTime RowVersion
);