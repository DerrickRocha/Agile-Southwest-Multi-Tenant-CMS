using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingMethod
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }  // "Standard Shipping", "Express", "Overnight"
    public string Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    
    // Pricing strategy
    public PricingStrategy PricingStrategy { get; set; }  // Flat, Weight, Price, Carrier, Free
    
    // Carrier integration (if applicable)
    public string? CarrierName { get; set; }  // "FedEx", "UPS", "USPS", null for custom
    public string? CarrierServiceCode { get; set; }  // "FEDEX_GROUND", "UPS_GROUND"
    
    // Estimated delivery
    public int EstimatedMinDays { get; set; }
    public int EstimatedMaxDays { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public ICollection<ShippingRateRule> RateRules { get; set; }
    public ICollection<ShippingRestriction> Restrictions { get; set; }
    public ICollection<ShippingZone> ShippingZones { get; set; }
}