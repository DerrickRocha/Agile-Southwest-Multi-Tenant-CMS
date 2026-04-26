namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record CreateOrderResult(
    int Id,
    string OrderNumber,
    int TotalCents
);