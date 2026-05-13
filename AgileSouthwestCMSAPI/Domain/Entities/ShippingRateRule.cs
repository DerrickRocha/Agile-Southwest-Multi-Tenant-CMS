using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingRateRule
{
    public int Id { get; set; }
    public int ShippingMethodId { get; set; }
    
    // Condition type (what triggers this rule)
    public RateConditionType ConditionType { get; set; }  // Subtotal, Weight, Quantity, Distance
    
    // Condition range (min and max inclusive)
    public decimal? MinValue { get; set; }  // Minimum subtotal, weight, etc.
    public decimal? MaxValue { get; set; }  // Maximum subtotal, weight, etc.
    
    // Pricing for this rule
    public decimal BasePriceCents { get; set; }
    public decimal? PricePerUnitCents { get; set; }  // Per kg, per $100, etc.
    public decimal? FreeShippingThresholdCents { get; set; }  // Override method-level free shipping
    
    // Priority (lower = checked first)
    public int Priority { get; set; }
}