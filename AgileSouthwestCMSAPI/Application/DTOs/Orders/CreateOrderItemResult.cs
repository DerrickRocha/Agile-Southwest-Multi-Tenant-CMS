namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record CreateOrderItemResult(
    string ProductName,
    int Quantity,
    int UnitPriceCents,
    int TotalPriceCents,
    IReadOnlyList<CreateOrderItemOptionResult> SelectedOptions
);