namespace AgileSouthwestCMSAPI.Domain.Entities;

public class TaxCategory
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public decimal TaxRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public Tenant Tenant { get; set; } = null!;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}