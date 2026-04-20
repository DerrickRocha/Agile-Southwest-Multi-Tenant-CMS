namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Store
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string SubDomain { get; set; }
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
}