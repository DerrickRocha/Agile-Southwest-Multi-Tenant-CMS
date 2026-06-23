namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record UpdateOrderResult(
    int Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string FulfillmentStatus,
    DateTime UpdatedAt
);