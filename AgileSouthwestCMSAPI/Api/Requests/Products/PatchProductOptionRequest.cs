namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record PatchProductOptionRequest(int? Id, string? Name, bool? IsRequired, ProductOptionChoiceRequest[]? Choices);