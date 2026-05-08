namespace AgileSouthwestCMSAPI.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string? ProductSku { get; set; }
    public int Quantity { get; set; }
    public int UnitPriceCents { get; set; }
    public int TotalPriceCents { get; set; }
    public int DiscountCents { get; set; }
    
    public decimal TaxRate { get; set; }
    
    // Product options snapshot
    public string? OptionDetails { get; set; } // JSON field
    
    // Image snapshot
    public string? ImageUrl { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}