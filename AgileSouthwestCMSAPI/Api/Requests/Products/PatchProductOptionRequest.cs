namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record PatchProductOptionRequest(
    int? Id = null,
    string? Name = null,
    bool? IsRequired = null,
    PatchProductOptionChoiceRequest[]? Choices = null
);