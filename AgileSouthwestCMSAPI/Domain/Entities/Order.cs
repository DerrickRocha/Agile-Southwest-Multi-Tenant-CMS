using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Order
{
   public int Id { get; set; }
   public int TenantId { get; set; }
   public int CustomerId { get; set; }
   public string OrderNumber { get; set; }
   public string CustomerEmail { get; set; }
   public string CustomerFirstName { get; set; }
   public string CustomerLastName { get; set; }
   public string? CustomerPhone { get; set; }
   public OrderStatus Status { get; set; }
   public PaymentStatus PaymentStatus { get; set; }
   public FulfillmentStatus FulfillmentStatus { get; set; }
       // Amounts (in cents)
    public int SubtotalCents { get; set; }
    public int TaxCents { get; set; }
    public int TotalCents { get; set; }

    public int DiscountCents { get; set; }
    public string? CouponCode { get; set; }
    public int CouponDiscountCents { get; set; }
    public int ShippingCents { get; set; }
    public int RefundedAmountCents { get; set; }
    public int PaymentServiceFeeCents { get; set; }
    
    // Currency
    public string Currency { get; set; } = "USD";
    
    // Shipping address
    public string ShippingAddressLine1 { get; set; }
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string ShippingPostalCode { get; set; }
    public string ShippingCountry { get; set; }
    
    // Billing address
    public string BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string BillingPostalCode { get; set; }
    public string BillingCountry { get; set; }
    
    // Payment info
    public string? PaymentProcessor { get; set; }
    public string? ProcessorTransactionId { get; set; }
    public string? ProcessorResponseCode { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? CheckoutSessionId { get; set; }
    public DateTime? PaymentAuthorizedAt { get; set; }
    public DateTime? PaymentCapturedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? PaymentExpiresAt { get; set; }
    public string? PaymentMethodDetails { get; set; } // JSON string
    public DateTime? PaymentSettledAt { get; set; }
    public int? PaymentRiskScore { get; set; }
    public string? PaymentMetadata { get; set; } // JSON string
    
    // Order type
    public OrderType OrderType { get; set; }
    
    // Audit
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Shipping info
    public int? ShippingMethodId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    
    // Notes
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    
    // Concurrency
    public DateTime RowVersion { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    public Tenant Tenant { get; set; } = null!;
    
    public Customer? Customer { get; set; }
    
}