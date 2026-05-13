using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.DTOs.Orders;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class OrdersService(ITenantContext context, CmsDbContext database, IHttpContextAccessor httpContextAccessor) : IOrderService
{
    public async Task<CreateOrderResult> CreateOrder(CreateOrderRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        if (request.BillingAddress is null) throw new ValidationException("Billing address cannot be null");
        if (request.PaymentMethod is null) throw new ValidationException("Payment method cannot be null");
        if (request.ShippingAddress is null) throw new ValidationException("Shipping address cannot be null");
        if (request.Items.Count <= 0) throw new ValidationException("Item count must be positive");

        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        
        var productIds = request.Items.Select(i => i.ProductId).Distinct();
        await using var transaction = await database.Database.BeginTransactionAsync();
        try
        {
            var products = await database.Products
                .Where(p => productIds.Contains(p.Id) && p.TenantId == tenant.Id && p.DeletedAt == null)
                .ToDictionaryAsync(p => p.Id);
            var orderItems = new List<OrderItem>();
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0) throw new ValidationException("Item quantity must be positive");
                if (!products.TryGetValue(item.ProductId, out var product))
                    throw new InvalidOperationException($"Product {item.ProductId} not found");
                var unitPrice = FindUnitPriceCents(product, item.SelectedOptions);
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    TenantId = tenant.Id,
                    ProductName = product.Name,
                    UnitPriceCents = unitPrice,
                    TotalPriceCents = unitPrice * item.Quantity,
                    TaxRate = product.TaxRate,
                };
                orderItems.Add(orderItem);
            }

            var subTotalCents = orderItems.Sum(i => i.UnitPriceCents * i.Quantity);
            var taxTotalCents = (int)Math.Round(
                orderItems.Sum(i => CalculateTaxCents(i.UnitPriceCents, i.Quantity, i.TaxRate)),
                MidpointRounding.AwayFromZero
            );
            var total = subTotalCents + taxTotalCents;

            // create order
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                TenantId = tenant.Id,
                OrderItems = orderItems,
                CustomerFirstName = request.CustomerFirstName,
                CustomerLastName = request.CustomerLastName,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Processing,
                FulfillmentStatus = FulfillmentStatus.Unfulfilled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TaxCents = taxTotalCents,
                SubtotalCents = subTotalCents,
                TotalCents = total,
                ShippingCents = await CalculateShippingCents(request.ShippingMethod, subTotalCents, orderItems, request.ShippingAddress),
                ShippingAddressLine1 = request.ShippingAddress.Line1,
                ShippingAddressLine2 = request.ShippingAddress.Line2,
                ShippingCity = request.ShippingAddress.City,
                ShippingState = request.ShippingAddress.State,
                ShippingPostalCode = request.ShippingAddress.PostalCode,
                ShippingCountry = request.ShippingAddress.Country,
                ShippingMethod = request.ShippingMethod,
                BillingAddressLine1 = request.BillingAddress.Line1,
                BillingAddressLine2 = request.BillingAddress.Line2,
                BillingCity = request.BillingAddress.City,
                BillingState = request.BillingAddress.State,
                BillingPostalCode = request.BillingAddress.PostalCode,
                BillingCountry = request.BillingAddress.Country,
                OrderType = OrderType.Standard,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CustomerNotes = request.CustomerNotes,
            };
            await database.Orders.AddAsync(order);
            await database.SaveChangesAsync();
            await transaction.CommitAsync();
            return new CreateOrderResult(order.Id, order.OrderNumber, order.TotalCents);
        }
        catch 
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<int> CalculateShippingCents(
    int? shippingMethodId, 
    int subtotalCents, 
    List<OrderItem> orderItems, 
    AddressRequest shippingAddress)
{
    // If no shipping method selected, assume free shipping or pickup
    if (shippingMethodId == null) return 0;
    
    // Fetch shipping method with all related data
    var shippingMethod = await database.ShippingMethods
        .Include(sm => sm.RateRules)
        .FirstOrDefaultAsync(sm => sm.Id == shippingMethodId && sm.IsActive);
    
    if (shippingMethod == null)
        throw new InvalidOperationException($"Shipping method {shippingMethodId} not found or inactive");
    
    // Check if shipping method is available for this address
    if (!await IsShippingMethodAvailableForAddressAsync(shippingMethod, shippingAddress))
        throw new ValidationException($"Shipping method {shippingMethod.Name} is not available for the selected address");
    
    // Check restrictions
    await ValidateShippingRestrictionsAsync(shippingMethod, orderItems, subtotalCents);
    
    // Calculate shipping cost based on pricing strategy
    var shippingCents = shippingMethod.PricingStrategy switch
    {
        PricingStrategy.Free => 0,
        PricingStrategy.Flat => CalculateFlatRateCents(shippingMethod, subtotalCents),
        PricingStrategy.Weight => await CalculateWeightBasedRateCentsAsync(shippingMethod, orderItems),
        PricingStrategy.Price => CalculatePriceBasedRateCents(shippingMethod, subtotalCents),
        PricingStrategy.Carrier => await CalculateCarrierRateCentsAsync(shippingMethod, orderItems, shippingAddress),
        _ => throw new NotSupportedException($"Pricing strategy {shippingMethod.PricingStrategy} not supported")
    };
    
    // Apply any free shipping overrides
    if (HasFreeShipping(shippingMethod, subtotalCents))
        return 0;
    
    return shippingCents;
}

// Helper methods
private async Task<bool> IsShippingMethodAvailableForAddressAsync(
    ShippingMethod shippingMethod, 
    AddressRequest address)
{
    // If no zones configured, method is available everywhere
    if (!shippingMethod.ShippingZones.Any())
        return true;
    
    // Check if address falls into any of the method's zones
    foreach (var zone in shippingMethod.ShippingZones)
    {
        if (await IsAddressInZoneAsync(address, zone))
            return true;
    }
    
    return false;
}

private async Task<bool> IsAddressInZoneAsync(AddressRequest address, ShippingZone zone)
{
    foreach (var location in zone.Locations)
    {
        switch (location.Type)
        {
            case LocationType.Country:
                if (address.Country.Equals(location.Code, StringComparison.OrdinalIgnoreCase))
                    return true;
                break;
                
            case LocationType.StateProvince:
                if (address.State?.Equals(location.Code, StringComparison.OrdinalIgnoreCase) == true)
                    return true;
                break;
                
            case LocationType.PostalCode:
                if (address.PostalCode?.StartsWith(location.Code) == true)
                    return true;
                break;
                
            case LocationType.Continent:
                var continent = GetContinentFromCountry(address.Country);
                if (continent.Equals(location.Code, StringComparison.OrdinalIgnoreCase))
                    return true;
                break;
        }
    }
    
    return false;
}

private async Task ValidateShippingRestrictionsAsync(
    ShippingMethod shippingMethod, 
    List<OrderItem> orderItems, 
    int subtotalCents)
{
    foreach (var restriction in shippingMethod.Restrictions)
    {
        switch (restriction.Type)
        {
            case RestrictionType.MaxWeightGrams:
                var totalWeightGrams = orderItems.Sum(i => i.WeightGrams * i.Quantity);
                if (totalWeightGrams > int.Parse(restriction.Value))
                    throw new ValidationException(restriction.Message ?? 
                        $"Order weight exceeds maximum of {restriction.Value}g for {shippingMethod.Name}");
                break;
                
            case RestrictionType.MaxQuantity:
                var totalQuantity = orderItems.Sum(i => i.Quantity);
                if (totalQuantity > int.Parse(restriction.Value))
                    throw new ValidationException(restriction.Message ?? 
                        $"Order quantity exceeds maximum of {restriction.Value} items for {shippingMethod.Name}");
                break;
                
            case RestrictionType.MinOrderValue:
                if (subtotalCents < int.Parse(restriction.Value))
                    throw new ValidationException(restriction.Message ?? 
                        $"Order subtotal of ${subtotalCents / 100m:F2} is below minimum of ${int.Parse(restriction.Value) / 100m:F2} for {shippingMethod.Name}");
                break;
                
            case RestrictionType.MaxOrderValue:
                if (subtotalCents > int.Parse(restriction.Value))
                    throw new ValidationException(restriction.Message ?? 
                        $"Order subtotal exceeds maximum for {shippingMethod.Name}");
                break;
                
            case RestrictionType.ExcludeLocation:
                // Handled by zone availability check
                break;
        }
    }
}

private int CalculateFlatRateCents(ShippingMethod shippingMethod, int subtotalCents)
{
    // Find applicable rate rule
    var rule = shippingMethod.RateRules
        .Where(r => r.ConditionType == RateConditionType.Subtotal)
        .OrderBy(r => r.Priority)
        .FirstOrDefault(r => subtotalCents >= (r.MinValue ?? 0) && 
                            (r.MaxValue == null || subtotalCents <= r.MaxValue));
    
    if (rule == null)
        rule = shippingMethod.RateRules.FirstOrDefault(r => r.ConditionType == RateConditionType.Subtotal);
    
    var baseRate = rule?.BasePriceCents ?? 0;
    
    // Apply free shipping threshold
    if (rule?.FreeShippingThresholdCents != null && 
        subtotalCents >= rule.FreeShippingThresholdCents)
        return 0;
    
    return (int)baseRate;
}

private async Task<int> CalculateWeightBasedRateCentsAsync(
    ShippingMethod shippingMethod, 
    List<OrderItem> orderItems)
{
    var totalWeightGrams = orderItems.Sum(i => (i.WeightGrams ?? 0) * i.Quantity);
    
    // Find applicable weight rule
    var rule = shippingMethod.RateRules
        .Where(r => r.ConditionType == RateConditionType.Weight)
        .OrderBy(r => r.Priority)
        .FirstOrDefault(r => totalWeightGrams >= (r.MinValue ?? 0) && 
                            (r.MaxValue == null || totalWeightGrams <= r.MaxValue));
    
    if (rule == null)
        throw new InvalidOperationException($"No weight rule found for shipping method {shippingMethod.Name}");
    
    var baseRate = rule.BasePriceCents;
    var additionalWeight = totalWeightGrams - (rule.MinValue ?? 0);
    var additionalCost = (additionalWeight / 100) * (rule.PricePerUnitCents ?? 0);
    
    return (int)(baseRate + additionalCost);
}

private int CalculatePriceBasedRateCents(ShippingMethod shippingMethod, int subtotalCents)
{
    var rule = shippingMethod.RateRules
        .Where(r => r.ConditionType == RateConditionType.Price)
        .OrderBy(r => r.Priority)
        .FirstOrDefault(r => subtotalCents >= (r.MinValue ?? 0) && 
                            (r.MaxValue == null || subtotalCents <= r.MaxValue));
    
    if (rule == null)
        rule = shippingMethod.RateRules.FirstOrDefault(r => r.ConditionType == RateConditionType.Price);
    
    var baseRate = rule?.BasePriceCents ?? 0;
    var additionalAmount = subtotalCents - (rule?.MinValue ?? 0);
    var additionalCost = (additionalAmount / 10000) * (rule?.PricePerUnitCents ?? 0); // Per $100
    
    return (int)(baseRate + additionalCost);
}

private async Task<int> CalculateCarrierRateCentsAsync(
    ShippingMethod shippingMethod, 
    List<OrderItem> orderItems, 
    AddressRequest shippingAddress)
{
    if (string.IsNullOrEmpty(shippingMethod.CarrierName) || 
        string.IsNullOrEmpty(shippingMethod.CarrierServiceCode))
    {
        throw new InvalidOperationException($"Carrier not configured for method {shippingMethod.Name}");
    }
    
    // Call your carrier service
    /*var carrierService = new CarrierRateService(); // Inject via DI
    var rate = await carrierService.GetRateAsync(new RateRequest
    {
        CarrierName = shippingMethod.CarrierName,
        ServiceCode = shippingMethod.CarrierServiceCode,
        OriginAddress = await GetWarehouseAddressAsync(),
        DestinationAddress = shippingAddress,
        Packages = MapOrderItemsToPackages(orderItems)
    });
    
    return rate.TotalCents;*/
    return 0;
}

private bool HasFreeShipping(ShippingMethod shippingMethod, int subtotalCents)
{
    // Check if any rate rule has free shipping threshold
    return shippingMethod.RateRules
        .Any(r => r.FreeShippingThresholdCents != null && 
                  subtotalCents >= r.FreeShippingThresholdCents);
}

private string GetContinentFromCountry(string countryCode)
{
    // Map country codes to continents
    var northAmerica = new[] { "US", "CA", "MX" };
    var europe = new[] { "GB", "DE", "FR", "IT", "ES", "NL", "BE" };
    // ... add more mappings
    
    if (northAmerica.Contains(countryCode)) return "NA";
    if (europe.Contains(countryCode)) return "EU";
    return "OTHER";
}

    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = RandomNumberGenerator.GetInt32(0, 999999);
        return $"ORD-{timestamp}-{random:D6}";
    }
    
    private static decimal CalculateTaxCents(int unitPriceCents, int quantity, decimal taxRatePercentage)
    {
        return unitPriceCents * quantity * (taxRatePercentage / 100m);
    }

    private int FindUnitPriceCents(Product product, Dictionary<int, int>? itemSelectedOptions)
    {
        var unitPrice = product.BasePriceCents;
        if (itemSelectedOptions == null || itemSelectedOptions.Count == 0)
        {
            return product.BasePriceCents;
        }

        unitPrice += (from option in itemSelectedOptions
            let productOption =
                product.ProductOptions.FirstOrDefault(o => o.Id == option.Key) ??
                throw new InvalidOperationException($"Product option {option.Key} not found")
            select productOption.ProductOptionChoices.FirstOrDefault(p => p.Id == option.Value) ??
                   throw new InvalidOperationException($"Product option choice {option.Value} not found")
            into choice
            select choice.PriceDeltaCents).Sum();

        return unitPrice;
    }

    public Task<GetOrderResult> GetOrder(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GetOrderResult>> GetOrders(OrderQueryParameters parameters)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateOrderResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        throw new NotImplementedException();
    }

    public Task CancelOrder(int id, string reason)
    {
        throw new NotImplementedException();
    }

    public Task<RefundResult> ProcessRefund(int id, RefundRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderStatusHistoryResult>> GetOrderHistory(int id)
    {
        throw new NotImplementedException();
    }
}