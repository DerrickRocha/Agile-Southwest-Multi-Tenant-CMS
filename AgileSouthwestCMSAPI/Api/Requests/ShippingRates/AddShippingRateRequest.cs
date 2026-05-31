namespace AgileSouthwestCMSAPI.Api.Requests.ShippingRates;

public record AddShippingRateRequest(
    int ShippingZoneId,
    string RateName,
    decimal MinWeight,
    decimal MaxWeight,
    int? PriceCents
);