namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Inventory
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public Product Product { get; set; } = null!;
}