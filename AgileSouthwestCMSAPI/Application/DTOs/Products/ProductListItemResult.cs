namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public record ProductListItemResult(
    int Id,
    string Name,
    int BasePrice,
    bool IsActive
);