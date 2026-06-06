namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record UpdateOrderStatusRequest(
    string Status,
    string PaymentStatus,
    string FulfillmentStatus,
    string? Reason
);