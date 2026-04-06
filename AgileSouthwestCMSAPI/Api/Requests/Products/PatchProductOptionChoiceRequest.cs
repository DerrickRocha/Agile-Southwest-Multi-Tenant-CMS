namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record PatchProductOptionChoiceRequest(
    int? Id = null,
    string? Name = null,
    int? PriceDelta = null,
    int? SalePriceDelta = null,
    bool? IsActive = null
);