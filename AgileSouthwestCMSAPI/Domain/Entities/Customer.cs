namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    
    public int UserId { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }   
    
    public CmsUser CmsUser { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();  // ADD THIS
}