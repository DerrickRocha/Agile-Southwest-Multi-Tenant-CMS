namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record UpdateOrderResult(
    int Id,
    string Status,
    DateTime UpdatedAt
);