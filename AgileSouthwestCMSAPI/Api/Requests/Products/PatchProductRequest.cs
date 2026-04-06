namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record PatchProductRequest(
    string? Name = null,
    string? Description = null,
    int? BasePrice = null,
    bool? IsActive = null,
    PatchProductOptionRequest[]? Options = null
);