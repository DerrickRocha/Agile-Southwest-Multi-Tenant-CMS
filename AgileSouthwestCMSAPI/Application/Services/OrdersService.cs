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

public class OrdersService(ITenantContext context, CmsDbContext database, IHttpContextAccessor httpContextAccessor)
    : IOrderService
{
    public async Task<CreateOrderResult> CreateOrder(CreateOrderRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        if (request.BillingAddress is null) throw new ValidationException("Billing address cannot be null");
        if (request.ShippingAddress is null) throw new ValidationException("Shipping address cannot be null");
        if (request.Items.Count <= 0) throw new ValidationException("Item count must be positive");

        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

        var productIds = request.Items.Select(i => i.ProductId).Distinct();
        var strategy = database.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
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
                        Product = product,
                        Quantity = item.Quantity,
                        Tenant = tenant,
                        ProductName = product.Name,
                        UnitPriceCents = unitPrice,
                        TotalPriceCents = unitPrice * item.Quantity,
                        TaxCategoryId = product.TaxCategoryId,
                    };
                    orderItems.Add(orderItem);
                }

                var subTotalCents = orderItems.Sum(i => i.UnitPriceCents * i.Quantity);
                var taxTotalCents = (int)Math.Round(
                    orderItems.Sum(i => CalculateTaxCents(i.UnitPriceCents, i.Quantity, i.TaxCategoryId??-1)),
                    MidpointRounding.AwayFromZero
                );
                var total = subTotalCents + taxTotalCents;
                /*var shippingCents = await CalculateShippingCents(request.ShippingRateId, subTotalCents, orderItems,
                    request.ShippingAddress);*/
                // create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    Tenant = tenant,
                    CustomerEmail = request.CustomerEmail,
                    CustomerFirstName = request.CustomerFirstName,
                    CustomerLastName = request.CustomerLastName,
                    CustomerPhone = request.CustomerPhone,

                    // Status tracking
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Processing, // Or Unpaid if not processing yet
                    FulfillmentStatus = FulfillmentStatus.Unfulfilled,

                    // Amounts (in cents)
                    SubtotalCents = subTotalCents,
                    DiscountCents = 0, // No discount applied yet
                    CouponCode = null, // No coupon by default
                    CouponDiscountCents = 0,
                    TaxCents = taxTotalCents,
                    ShippingCents = 0,
                    TotalCents = total + 0, // Don't forget to include shipping!
                    RefundedAmountCents = 0,
                    PaymentServiceFeeCents = 0,

                    // Currency
                    Currency = nameof(Currency.Usd),

                    // Shipping address
                    ShippingAddressLine1 = request.ShippingAddress.Line1,
                    ShippingAddressLine2 = request.ShippingAddress.Line2,
                    ShippingCity = request.ShippingAddress.City,
                    ShippingState = request.ShippingAddress.State,
                    ShippingPostalCode = request.ShippingAddress.PostalCode,
                    ShippingCountry = request.ShippingAddress.Country,

                    // Billing address
                    BillingAddressLine1 = request.BillingAddress.Line1,
                    BillingAddressLine2 = request.BillingAddress.Line2,
                    BillingCity = request.BillingAddress.City,
                    BillingState = request.BillingAddress.State,
                    BillingPostalCode = request.BillingAddress.PostalCode,
                    BillingCountry = request.BillingAddress.Country,

                    // Order type
                    OrderType = OrderType.Standard,

                    // Audit
                    IpAddress = ipAddress,
                    UserAgent = userAgent,

                    // Shipping info

                    // Notes
                    CustomerNotes = request.CustomerNotes,
                    AdminNotes = null,

                    // Navigation properties
                    OrderItems = orderItems,
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
        });
    }

  /*  private async Task<int> CalculateShippingCents(
        int? shippingRateId,
        int subtotalCents,
        List<OrderItem> orderItems,
        AddressRequest shippingAddress)
    {
        if (shippingRateId == null) return 0;

        var shippingRate = await database.ShippingRates.FirstOrDefaultAsync(sr => sr.Id == shippingRateId);
        if (shippingRate == null) throw new InvalidOperationException($"Shipping rate {shippingRate} not found");

        if (shippingRate.MinWeight == 0 && shippingRate.MaxWeight == 0) return shippingRate.PriceCents;

        var weights = orderItems.Select(i => (i.WeightGrams * i.Quantity));
        var totalWeight = weights.Sum();
        if (totalWeight < shippingRate.MinWeight || totalWeight > shippingRate.MaxWeight)
            throw new InvalidOperationException(
                $"Order weight {totalWeight} is not within the allowed range of {shippingRate.MinWeight} to {shippingRate.MaxWeight} grams");

        return shippingRate.PriceCents;
    }*/

    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = RandomNumberGenerator.GetInt32(0, 999999);
        return $"ORD-{timestamp}-{random:D6}";
    }

    private decimal CalculateTaxCents(int unitPriceCents, int quantity, int taxCategoryId)
    {
        var taxCategory = database.TaxCategories.FirstOrDefault(tc => tc.Id == taxCategoryId) ??
                          throw new InvalidOperationException($"Tax category {taxCategoryId} not found");
        var taxRatePercentage = taxCategory.TaxRate;
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