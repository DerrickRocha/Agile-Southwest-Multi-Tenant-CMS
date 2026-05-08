namespace AgileSouthwestCMSAPI.Domain.Enums;

public enum PaymentStatus
{
    Unpaid,           // No payment attempted
    Authorized,       // Card authorized, not captured
    Processing,       // ACH payment in progress (awaiting settlement)
    Paid,             // Payment completed
    Failed,           // Payment failed
    Refunded,         // Fully refunded
    PartialRefunded
}