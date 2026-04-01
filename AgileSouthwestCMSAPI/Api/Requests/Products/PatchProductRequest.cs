
namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record PatchProductRequest(
    string? Name,
    string? Description,
    int? BasePrice,
    bool? IsActive,
    ProductOptionRequest[]? Options
);