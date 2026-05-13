using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ShippingRestriction
{
    public int Id { get; set; }
    public int ShippingMethodId { get; set; }
    
    public RestrictionType Type { get; set; }  // ExcludeLocation, MaxWeight, MaxQuantity, ExcludeProductCategory
    public string? Value { get; set; }  // JSON or simple value based on type
    public string? Message { get; set; }  // Error message shown to customer
}