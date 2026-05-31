using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Order
{
       public int Id { get; set; }
    public int TenantId { get; set; }
    public int? CustomerId { get; set; }
    public int ShippingZoneId { set; get; }
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
    public string Currency { get; set; } 
    
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
    
    public OrderType OrderType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    // Concurrency
    public DateTime RowVersion { get; set; }
    
    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<PaymentTransactions> PaymentTransactions { get; set; } = new List<PaymentTransactions>();
    public Tenant Tenant { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ShippingZone ShippingZone { set; get; }

}