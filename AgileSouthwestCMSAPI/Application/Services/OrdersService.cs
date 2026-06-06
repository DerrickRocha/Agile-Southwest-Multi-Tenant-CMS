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
                    orderItems.Sum(i => CalculateTaxCents(i.UnitPriceCents, i.Quantity, i.TaxCategoryId ?? -1)),
                    MidpointRounding.AwayFromZero
                );
                var total = subTotalCents + taxTotalCents;
                var shippingCents = await CalculateShippingCents(request.ShippingRateId, subTotalCents, orderItems,
                    request.ShippingAddress);
                // create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    Tenant = tenant,
                    ShippingRateId = request.ShippingRateId ?? -1,
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
                    ShippingCents = shippingCents,
                    TotalCents = total + shippingCents, // Don't forget to include shipping!
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
                return new CreateOrderResult(
                    Id: order.Id,
                    OrderNumber: order.OrderNumber,
                    CustomerEmail: order.CustomerEmail,
                    CustomerFirstName: order.CustomerFirstName,
                    CustomerLastName: order.CustomerLastName,
                    CustomerPhone: order.CustomerPhone ?? "",
                    Status: order.Status.ToString(),
                    PaymentStatus: order.PaymentStatus.ToString(),
                    FulfillmentStatus: order.FulfillmentStatus.ToString() ?? "",
                    SubtotalCents: order.SubtotalCents,
                    DiscountCents: order.DiscountCents,
                    TaxCents: order.TaxCents,
                    ShippingCents: order.ShippingCents,
                    TotalCents: order.TotalCents,
                    CouponCode: order.CouponCode ?? "",
                    CouponDiscountCents: order.CouponDiscountCents,
                    RefundedAmountCents: order.RefundedAmountCents,
                    PaymentServiceFeeCents: order.PaymentServiceFeeCents,
                    Currency: order.Currency,
                    ShippingAddressLine1: order.ShippingAddressLine1,
                    ShippingAddressLine2: order.ShippingAddressLine2 ?? "",
                    ShippingCity: order.ShippingCity,
                    ShippingState: order.ShippingState ?? "",
                    ShippingPostalCode: order.ShippingPostalCode,
                    ShippingCountry: order.ShippingCountry,
                    BillingAddressLine1: order.BillingAddressLine1,
                    BillingAddressLine2: order.BillingAddressLine2 ?? "",
                    BillingCity: order.BillingCity,
                    BillingState: order.BillingState ?? "",
                    BillingPostalCode: order.BillingPostalCode,
                    BillingCountry: order.BillingCountry,
                    OrderType: order.OrderType.ToString(),
                    IpAddress: order.IpAddress ?? "",
                    UserAgent: order.UserAgent ?? "",
                    CustomerNotes: order.CustomerNotes ?? "",
                    AdminNotes: order.AdminNotes ?? "",
                    CreatedAt: order.CreatedAt,
                    UpdatedAt: order.UpdatedAt,
                    DeletedAt: order.DeletedAt,
                    RowVersion: order.RowVersion
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task<int> CalculateShippingCents(
        int? shippingRateId,
        int subtotalCents,
        List<OrderItem> orderItems,
        AddressRequest shippingAddress)
    {
        if (shippingRateId == null) return 0;

        var shippingRate = await database.ShippingRates.FirstOrDefaultAsync(sr => sr.Id == shippingRateId);
        if (shippingRate == null) throw new InvalidOperationException($"Shipping rate {shippingRate} not found");

        if (shippingRate.MinWeightGrams == 0 && shippingRate.MaxWeightGrams == 0) return shippingRate.PriceCents;

        var weights = orderItems.Select(i => (i.WeightGrams * i.Quantity));
        var totalWeight = weights.Sum();
        if (totalWeight < shippingRate.MinWeightGrams || totalWeight > shippingRate.MaxWeightGrams)
            throw new InvalidOperationException(
                $"Order weight {totalWeight} is not within the allowed range of {shippingRate.MinWeightGrams} to {shippingRate.MaxWeightGrams} grams");

        return shippingRate.PriceCents;
    }

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

    public async Task<GetOrderResult> GetOrder(int id)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var order = await database.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenant.Id);

        if (order == null)
            throw new InvalidOperationException($"Order with id {id} not found");

        return MapToGetOrderResult(order);
    }

    private GetOrderResult MapToGetOrderResult(Order order)
    {
        return new GetOrderResult(
            Id: order.Id,
            TenantId: order.TenantId,
            ShippingRateId: order.ShippingRateId,
            OrderNumber: order.OrderNumber,
            CustomerId: order.CustomerId,
            CustomerEmail: order.CustomerEmail,
            CustomerFirstName: order.CustomerFirstName,
            CustomerLastName: order.CustomerLastName,
            CustomerPhone: order.CustomerPhone,
            Status: order.Status.ToString(),
            PaymentStatus: order.PaymentStatus.ToString(),
            FulfillmentStatus: order.FulfillmentStatus.ToString() ?? "",
            SubtotalCents: order.SubtotalCents,
            DiscountCents: order.DiscountCents,
            TaxCents: order.TaxCents,
            ShippingCents: order.ShippingCents,
            TotalCents: order.TotalCents,
            Currency: order.Currency,
            ShippingAddressLine1: order.ShippingAddressLine1,
            ShippingAddressLine2: order.ShippingAddressLine2 ?? "",
            ShippingCity: order.ShippingCity,
            ShippingState: order.ShippingState ?? "",
            ShippingPostalCode: order.ShippingPostalCode,
            ShippingCountry: order.ShippingCountry,
            BillingAddressLine1: order.BillingAddressLine1,
            BillingAddressLine2: order.BillingAddressLine2 ?? "",
            BillingCity: order.BillingCity,
            BillingState: order.BillingState ?? "",
            BillingPostalCode: order.BillingPostalCode,
            BillingCountry: order.BillingCountry,
            Items: order.OrderItems.Select(oi => new OrderItemDto(
                Id: oi.Id,
                ProductId: oi.ProductId,
                ProductName: oi.ProductName ?? "",
                ProductSku: oi.ProductSku,
                Quantity: oi.Quantity,
                UnitPriceCents: oi.UnitPriceCents,
                TotalPriceCents: oi.TotalPriceCents,
                OptionDetails: oi.OptionDetails ?? ""
            )),
            CustomerNotes: order.CustomerNotes,
            AdminNotes: order.AdminNotes,
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            DeletedAt: order.DeletedAt,
            RowVersion: order.RowVersion
        );
    }

    public async Task<IEnumerable<GetOrderResult>> GetOrders(OrderQueryParameters parameters)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var query = database.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.TenantId == tenant.Id);

        // Apply filters
        if (!string.IsNullOrEmpty(parameters.OrderNumber))
            query = query.Where(o => o.OrderNumber.Contains(parameters.OrderNumber));

        if (parameters.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == parameters.CustomerId);

        if (!string.IsNullOrEmpty(parameters.CustomerEmail))
            query = query.Where(o => o.CustomerEmail.Contains(parameters.CustomerEmail));

        if (!string.IsNullOrEmpty(parameters.Status))
            query = query.Where(o => o.Status.ToString() == parameters.Status);

        if (!string.IsNullOrEmpty(parameters.PaymentStatus))
            query = query.Where(o => o.PaymentStatus.ToString() == parameters.PaymentStatus);

        if (!string.IsNullOrEmpty(parameters.FulfillmentStatus))
            query = query.Where(o => o.FulfillmentStatus.ToString() == parameters.FulfillmentStatus);

        if (parameters.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= parameters.FromDate);

        if (parameters.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= parameters.ToDate);

        if (parameters.MinTotalCents.HasValue)
            query = query.Where(o => o.TotalCents >= parameters.MinTotalCents);

        if (parameters.MaxTotalCents.HasValue)
            query = query.Where(o => o.TotalCents <= parameters.MaxTotalCents);

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "ordernumber" => parameters.SortDescending
                ? query.OrderByDescending(o => o.OrderNumber)
                : query.OrderBy(o => o.OrderNumber),
            "total" => parameters.SortDescending
                ? query.OrderByDescending(o => o.TotalCents)
                : query.OrderBy(o => o.TotalCents),
            "status" => parameters.SortDescending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            _ => parameters.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt)
        };

        query = query.Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize);

        var orders = await query.ToListAsync();
        return orders.Select(MapToGetOrderResult);
    }

    public async Task<UpdateOrderResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var order = await database.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenant.Id);

        if (order == null)
            throw new KeyNotFoundException($"Order with id {id} not found");

        // Track old status for history
        var oldStatus = order.Status;
        var oldPaymentStatus = order.PaymentStatus;
        var oldFulfillmentStatus = order.FulfillmentStatus;

        // Update statuses
        if (!string.IsNullOrEmpty(request.Status))
            order.Status = Enum.Parse<OrderStatus>(request.Status);

        if (!string.IsNullOrEmpty(request.PaymentStatus))
            order.PaymentStatus = Enum.Parse<PaymentStatus>(request.PaymentStatus);

        if (!string.IsNullOrEmpty(request.FulfillmentStatus))
            order.FulfillmentStatus = Enum.Parse<FulfillmentStatus>(request.FulfillmentStatus);

        // Add status history entry
        var statusHistory = new OrderStatusHistory
        {
            Tenant = tenant,
            Order = order,
            ChangedByUser = context.User,
            OldStatus = oldStatus.ToString(),
            NewStatus = order.Status.ToString(),
            OldPaymentStatus = oldPaymentStatus.ToString(),
            NewPaymentStatus = order.PaymentStatus.ToString(),
            OldFulfillmentStatus = oldFulfillmentStatus.ToString() ?? "",
            NewFulfillmentStatus = order.FulfillmentStatus.ToString() ?? "",
            ChangedBy = context.User?.Id ?? -1,
            Reason = request.Reason ?? "",
        };

        await database.OrderStatusHistories.AddAsync(statusHistory);
        await database.SaveChangesAsync();

        return new UpdateOrderResult(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            Status: order.Status.ToString(),
            PaymentStatus: order.PaymentStatus.ToString(),
            FulfillmentStatus: order.FulfillmentStatus.ToString(),
            UpdatedAt: order.UpdatedAt
        );
    }

    public async Task CancelOrder(int id, string reason)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
    
        var order = await database.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenant.Id);
    
        if (order == null)
            throw new KeyNotFoundException($"Order with id {id} not found");
    
        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");
    
        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed order");
    
        // Track old status for history
        var oldStatus = order.Status;
        var oldPaymentStatus = order.PaymentStatus;
        var oldFulfillmentStatus = order.FulfillmentStatus;
    
        // Update order
        order.Status = OrderStatus.Cancelled;
        order.PaymentStatus = PaymentStatus.Refunded;
        order.FulfillmentStatus = FulfillmentStatus.Unfulfilled;
        order.AdminNotes = string.IsNullOrEmpty(order.AdminNotes) 
            ? $"Cancelled: {reason}" 
            : $"{order.AdminNotes}\nCancelled: {reason}";
    
        // Add cancellation to history
        var statusHistory = new OrderStatusHistory
        {
            Tenant = tenant,
            Order = order,
            ChangedByUser = context.User,
            OldStatus = oldStatus.ToString(),
            NewStatus = nameof(OrderStatus.Cancelled),
            OldPaymentStatus = oldPaymentStatus.ToString(),
            NewPaymentStatus = nameof(PaymentStatus.Refunded),
            OldFulfillmentStatus = oldFulfillmentStatus?.ToString()?? "",
            NewFulfillmentStatus = nameof(FulfillmentStatus.Unfulfilled),
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    
        await database.OrderStatusHistories.AddAsync(statusHistory);
        await database.SaveChangesAsync();
    }

    public Task<RefundResult> ProcessRefund(int id, RefundRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<OrderStatusHistoryResult>> GetOrderHistory(int id)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
    
        var order = await database.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenant.Id);
    
        if (order == null)
            throw new KeyNotFoundException($"Order with id {id} not found");
    
        var history = await database.OrderStatusHistories
            .Where(h => h.OrderId == id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    
        return history.Select(h => new OrderStatusHistoryResult(
            Id: h.Id,
            OrderId: h.OrderId,
            OldStatus: h.OldStatus,
            NewStatus: h.NewStatus,
            OldPaymentStatus: h.OldPaymentStatus,
            NewPaymentStatus: h.NewPaymentStatus,
            OldFulfillmentStatus: h.OldFulfillmentStatus,
            NewFulfillmentStatus: h.NewFulfillmentStatus,
            ChangedBy: h.ChangedBy,
            Reason: h.Reason,
            CreatedAt: h.CreatedAt
        ));
    }
}