namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingZone
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }  // "Domestic", "Canada", "International"
    public bool IsLocalFleet { set; get; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }   
    
    // Navigation
    
    public Tenant Tenant { get; set; } = null!;

    public ICollection<ZonePostalCodes> ZonePostalCodes { get; set; }
    public ICollection<ShippingRate> ShippingRates { get; set; }  // Many-to-many via junction
}