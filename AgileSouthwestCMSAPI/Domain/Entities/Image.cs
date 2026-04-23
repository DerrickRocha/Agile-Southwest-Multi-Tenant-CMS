namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Image
{
    public int Id { set; get; }
    
    public int TenantId { set; get; }
    
    public string Url { set; get; } 
    
    public string Description { set; get; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
}