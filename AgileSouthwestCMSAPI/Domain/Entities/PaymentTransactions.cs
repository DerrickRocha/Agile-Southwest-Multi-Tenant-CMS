using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class PaymentTransactions
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int OrderId { get; set; }
    public int AmountCents { get; set; }
    public TransactionType TransactionType { get; set; }
    public Currency Currency { get; set; }
    public GatewayName GatewayName { get; set; }
    public string? GatewayTransactionId { get; set; }
    public int? GatewayFeeCents { get; set; }
    public PaymentTransactionStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? RawGatewayResponse { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime RowVersion { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; }
    public Order Order { get; set; }   
}