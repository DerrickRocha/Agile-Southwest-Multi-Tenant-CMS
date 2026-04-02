namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null
);