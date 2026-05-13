namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingZone
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }  // "Domestic", "Canada", "International"
    public bool IsActive { get; set; }
    
    // Navigation
    public ICollection<ShippingZoneLocation> Locations { get; set; }
    public ICollection<ShippingMethod> ShippingMethods { get; set; }  // Many-to-many via junction
}