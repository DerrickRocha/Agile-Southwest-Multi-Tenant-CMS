using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Order
{
   public int Id { get; set; }
    public int TenantId { get; set; }
    

    public int? CustomerId { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerFirstName { get; set; }
    public string CustomerLastName { get; set; }
    public string? CustomerPhone { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public FulfillmentStatus? FulfillmentStatus { get; set; }
    public int SubtotalCents { get; set; }
    public int DiscountCents { get; set; }
    public string? CouponCode { get; set; }
    public int CouponDiscountCents { get; set; }
    public int TaxCents { get; set; }
    public int ShippingCents { get; set; }
    public int TotalCents { get; set; }
    public int RefundedAmountCents { get; set; }
    public int PaymentServiceFeeCents { get; set; }
    public string Currency { get; set; }
    public string ShippingAddressLine1 { get; set; }
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string ShippingPostalCode { get; set; }
    public string ShippingCountry { get; set; }
    public string BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string BillingPostalCode { get; set; }
    public string BillingCountry { get; set; }
    public OrderType OrderType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime RowVersion { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    public ICollection<PaymentTransactions> PaymentTransactions { get; set; } = new List<PaymentTransactions>();

}