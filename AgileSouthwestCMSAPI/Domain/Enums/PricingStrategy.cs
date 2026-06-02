namespace AgileSouthwestCMSAPI.Domain.Enums;

public enum PricingStrategy
{
    Flat = 1,      // Simple fixed price
    Weight = 2,    // Based on total weight
    Price = 3,     // Based on order subtotal
    Carrier = 4,   // Real-time rates from carrier API
    Free = 5       // Always free
}