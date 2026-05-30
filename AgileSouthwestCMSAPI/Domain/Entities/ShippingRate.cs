namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingRate
{
    public int Id { set; get; }
    public int TenantId { set; get; }
    public int ShippingZoneId { set; get; }
    public string RateName { set; get; }
    public decimal MinWeight { set; get; }
    public decimal MaxWeight { set; get; }
    public int PriceCents { set; get; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }   
    
    public Tenant Tenant { set; get; }
    public ShippingZone ShippingZone { set; get; }
}