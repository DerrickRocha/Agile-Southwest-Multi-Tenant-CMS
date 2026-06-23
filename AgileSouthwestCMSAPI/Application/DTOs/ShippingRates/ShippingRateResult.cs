namespace AgileSouthwestCMSAPI.Application.DTOs.ShippingRates;

public record ShippingRateResult(
    int Id,
    int TenantId,
    string RateName,
    decimal MinWeight,
    decimal MaxWeight,
    int PriceCents,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
);