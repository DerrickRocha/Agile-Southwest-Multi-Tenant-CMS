namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record PatchProductOptionChoiceRequest(int? Id, string? Name, int? PriceDelta, int? SalePriceDelta, bool? IsActive);