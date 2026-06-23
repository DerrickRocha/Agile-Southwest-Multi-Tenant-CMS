namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record RefundResult(
    int OrderId,
    int AmountCents,
    string PaymentStatus
);