namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ProductImage
{
    public int Id { set; get; }
    
    public int TenantId { set; get; }
    
    public int ProductId { set; get; }
    
    public int ImageId { set; get; }
    
    public bool IsPrimary { set; get; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
}