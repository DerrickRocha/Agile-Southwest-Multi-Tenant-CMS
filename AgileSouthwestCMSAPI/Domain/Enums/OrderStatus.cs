namespace AgileSouthwestCMSAPI.Domain.Enums;

public enum OrderStatus
{
    Pending,
    AwaitingPayment,
    PaymentProcessing,
    Paid,
    PaymentFailed,
    PaymentExpired,
    PartiallyRefunded,
    Refunded,
    Cancelled,
}