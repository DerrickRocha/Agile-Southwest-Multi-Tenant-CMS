namespace AgileSouthwestCMSAPI.Domain.Enums;

public enum PaymentStatus
{
    Unpaid,           
    Paid,           
    Processing,    
    PartiallyPaid,
    Failed,     
    PaymentExpired,
    Refunded,         
    PartialRefunded
}