namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record OrderStatusHistoryResult(
    int OrderId,
    int AmountCents,
    string PaymentStatus
);