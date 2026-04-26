namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string? ProductSku,
    int Quantity,
    int UnitPriceCents,
    int TotalPriceCents,
    Dictionary<string, string>? OptionDetails
);