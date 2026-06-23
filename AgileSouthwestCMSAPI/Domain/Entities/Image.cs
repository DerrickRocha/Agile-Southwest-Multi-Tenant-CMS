namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Image
{
    public int Id { set; get; }
    
    public int TenantId { set; get; }
    
    public string Url { set; get; } 
    
    public string? OriginalFileName { set; get; }
    
    public long? FileSize { get; set; }   
    
    public string? ContentType { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }    
    
    public Tenant Tenant { get; set; }
    
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}