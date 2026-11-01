namespace AgileSouthwestCMSAPI.Api.Requests.ShippingRates;

public record AddShippingRateRequest(
    string RateName,
    decimal MinWeight,
    decimal MaxWeight,
    int? PriceCents,
    string? PostalCode
);