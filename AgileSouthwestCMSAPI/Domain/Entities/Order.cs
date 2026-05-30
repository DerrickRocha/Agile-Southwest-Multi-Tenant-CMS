using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Order
{
       public int Id { get; set; }
    public int TenantId { get; set; }
    public int? CustomerId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string? CustomerPhone { get; set; }
    
    // Status tracking
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public FulfillmentStatus FulfillmentStatus { get; set; }
    
    // Amounts (in cents)
    public int SubtotalCents { get; set; }
    public int DiscountCents { get; set; }
    public string? CouponCode { get; set; }
    public int CouponDiscountCents { get; set; }
    public int TaxCents { get; set; }
    public int ShippingCents { get; set; }
    public int TotalCents { get; set; }
    public int RefundedAmountCents { get; set; }
    public int PaymentServiceFeeCents { get; set; }
    
    // Currency
    public Currency? Currency { get; set; }  // NULL allowed in DB
    
    // Shipping address
    public string ShippingAddressLine1 { get; set; } = null!;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = null!;
    public string? ShippingState { get; set; }
    public string ShippingPostalCode { get; set; } = null!;
    public string ShippingCountry { get; set; } = null!;
    
    // Billing address
    public string BillingAddressLine1 { get; set; } = null!;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = null!;
    public string? BillingState { get; set; }
    public string BillingPostalCode { get; set; } = null!;
    public string BillingCountry { get; set; } = null!;
    
    // Payment info
    public PaymentProcessor? PaymentProcessor { get; set; }
    public string? ProcessorTransactionId { get; set; }
    public string? ProcessorResponseCode { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? CheckoutSessionId { get; set; }
    public DateTime? PaymentAuthorizedAt { get; set; }
    public DateTime? PaymentCapturedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? PaymentExpiresAt { get; set; }
    public string? PaymentMethodDetails { get; set; }
    public DateTime? PaymentSettledAt { get; set; }
    public int? PaymentRiskScore { get; set; }
    public string? PaymentMetadata { get; set; }
    
    // Order type
    public OrderType OrderType { get; set; }
    
    // Audit
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Shipping info
    public int? ShippingMethodId { get; set; }
    public string? ShippingMethod { get; set; }  // Legacy field - make nullable
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    
    // Notes
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    
    // Timestamps - These are NOT nullable in DB but have defaults
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    
    // Concurrency
    public DateTime RowVersion { get; set; }
    
    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Tenant Tenant { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ShippingMethod? ShippingMethodNavigation { get; set; }

}