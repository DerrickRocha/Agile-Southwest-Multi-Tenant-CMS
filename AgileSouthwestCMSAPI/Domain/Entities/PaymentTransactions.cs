using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class PaymentTransactions
{
    public int Id { set; get; }
    public int TenantId { set; get; }   
    public int OrderId { set; get; }    
    public int AmountCents { set; get; }
    public TransactionType TransactionType { set; get; }
    public Currency Currency { set; get; }
    public GatewayName GatewayName { set; get; }
    public string GatewayTransactionId { get; set; }
    public int GatewayFeeCents { get; set; }
    public PaymentTransactionStatus Status { set; get; }
    public string ErrorCode { get; set; }
    public string RawGatewayResponse { set; get; }
    public string? ErrorMessage { set; get; }
    public DateTime CreatedAt { set; get; }
    public DateTime UpdatedAt { set; get; }
    public DateTime? DeletedAt { set; get; }
    public DateTime RowVersion { set; get; }   
    
    public Tenant Tenant { set; get; }
    public Order Order { set; get; }   
}