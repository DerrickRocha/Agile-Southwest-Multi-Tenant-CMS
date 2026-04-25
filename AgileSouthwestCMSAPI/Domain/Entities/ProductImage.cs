namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ProductImage
{
    public int Id { set; get; }
    
    public int TenantId { set; get; }
    
    public int ProductId { set; get; }
    
    public int ImageId { set; get; }
    
    public bool IsPrimary { set; get; }
    
    public int Position { set; get; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; }
    public virtual Product Product { get; set; }
    public virtual Image Image { get; set; }
}