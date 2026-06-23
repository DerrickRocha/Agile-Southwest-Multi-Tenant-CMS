namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingRate
{
    public int Id { set; get; }
    public int TenantId { set; get; }
    public string RateName { set; get; }
    public string PostalCode { set; get; }
    public decimal MinWeightGrams { set; get; }
    public decimal? MaxWeightGrams { set; get; }
    public int PriceCents { set; get; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }   
    
    public Tenant Tenant { set; get; }
    
    public ICollection<Order> Orders { set; get; } = new List<Order>();
}