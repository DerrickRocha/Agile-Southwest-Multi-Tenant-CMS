using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.DTOs.Orders;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MySql.Data.MySqlClient;

namespace AgileSouthwestCMSAPI.Application.Services;

public class OrdersService(
    ITenantContext context,
    CmsDbContext database,
    IHttpContextAccessor httpContextAccessor,
    ILogger<OrdersService> logger
)
    : IOrderService
{
    private const string CurrencyCode = "USD";
    private const int MaxOrderNumberAttempts = 3;
    private const int MaxItems = 100;
    private const int MaxQuantity = 1000;
    private const int MaxOptionsPerItem = 20;

    public async Task<CreateOrderResult> CreateOrder(CreateOrderRequest request, CancellationToken ct = default)
    {
        // Tenant resolution should be enforced by TenantResolutionMiddleware; this is a safety net.
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var tenantId = tenant.Id;

        // 1. Validate everything before touching the database.
        var email = ValidateRequest(request);
        var idempotencyKey = Clean(request.IdempotencyKey);

        // 2. Replay: the same key returns the order that was already created.
        if (idempotencyKey is not null)
        {
            var existing = await FindByIdempotencyKeyAsync(tenantId, idempotencyKey, ct);
            if (existing is not null) return ToResult(existing);
        }

        // 3. Price from the database, never from the client.
        var priced = await PriceOrderAsync(tenantId, request, ct);

        // 4. Persist.
        var order = await PersistAsync(tenantId, request, email, idempotencyKey, priced, ct);
        return ToResult(order);
    }

    private async Task<PricedOrder> PriceOrderAsync(int tenantId, CreateOrderRequest request, CancellationToken ct)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await database.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.ProductOptions).ThenInclude(o => o.ProductOptionChoices)
            .Where(p => productIds.Contains(p.Id) && p.TenantId == tenantId && p.IsActive && p.DeletedAt == null)
            .ToDictionaryAsync(p => p.Id, ct);

        // One query for all tax rates instead of one per line item.
        var taxCategoryIds = products.Values.Select(p => p.TaxCategoryId).Distinct().ToList();
        var taxRates = await database.TaxCategories
            .AsNoTracking()
            .Where(tc => taxCategoryIds.Contains(tc.Id) && tc.TenantId == tenantId)
            .ToDictionaryAsync(tc => tc.Id, tc => tc.TaxRate, ct);

        var items = new List<PricedItem>(request.Items.Count);
        foreach (var line in request.Items)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                throw new ValidationException($"Product {line.ProductId} is not available.");

            var (unitPriceCents, options) = PriceItem(product, line.SelectedOptions);
            var lineTotal = (long)unitPriceCents * line.Quantity;

            // A missing category is bad catalog data, not bad input: let it surface as a logged 500.
            var rate = taxRates.TryGetValue(product.TaxCategoryId, out var r)
                ? r
                : throw new InvalidDataException(
                    $"Tax category {product.TaxCategoryId} for product {product.Id} does not exist.");

            // Round per line so stored line taxes add up to the order tax.
            var taxCents = ToCents(Math.Round(lineTotal * rate / 100m, MidpointRounding.AwayFromZero));

            items.Add(new PricedItem(
                ProductId: product.Id,
                ProductName: product.Name,
                Quantity: line.Quantity,
                UnitPriceCents: unitPriceCents,
                TotalPriceCents: ToCents(lineTotal),
                TaxCategoryId: product.TaxCategoryId,
                TaxCents: taxCents,
                WeightGrams: product.WeightGrams,
                RequiresShipping: product.RequiresShipping,
                Options: options));
        }

        var subtotalCents = ToCents(items.Sum(i => (long)i.TotalPriceCents));
        var taxTotalCents = ToCents(items.Sum(i => (long)i.TaxCents));

        var (shippingRateId, shippingCents) =
            await ResolveShippingAsync(tenantId, request.ShippingRateId, items, request.ShippingAddress!, ct);

        var totalCents = ToCents((long)subtotalCents + taxTotalCents + shippingCents);

        return new PricedOrder(items, subtotalCents, taxTotalCents, shippingCents, shippingRateId, totalCents);
    }

    private async Task<(int? RateId, int Cents)> ResolveShippingAsync(
        int tenantId,
        int? shippingRateId,
        IReadOnlyList<PricedItem> items,
        AddressRequest shippingAddress,
        CancellationToken ct)
    {
        // Whether shipping applies is decided by the products, not by what the client sent.
        if (!items.Any(i => i.RequiresShipping)) return (null, 0);

        if (shippingRateId is null)
            throw new ValidationException("A shipping rate is required.");

        var rate = await database.ShippingRates
                       .AsNoTracking()
                       .FirstOrDefaultAsync(
                           sr => sr.Id == shippingRateId && sr.TenantId == tenantId && sr.DeletedAt == null, ct)
                   ?? throw new ValidationException("The selected shipping rate is not available.");

        // Rates carry a postal code, so make sure the rate applies to this destination.
        // Assumes an exact match; change this if PostalCode is a prefix or a zone.
        if (NormalizePostalCode(rate.PostalCode) != NormalizePostalCode(shippingAddress.PostalCode))
            throw new ValidationException("The selected shipping rate does not apply to this address.");

        // MinWeightGrams is always set; a null MaxWeightGrams means no upper limit.
        var totalWeight = items.Sum(i => (long)(i.WeightGrams ?? 0) * i.Quantity);
        if (totalWeight < rate.MinWeightGrams || (rate.MaxWeightGrams is { } max && totalWeight > max))
            throw new ValidationException(
                $"Order weight ({totalWeight} g) is outside the range for the selected shipping rate.");

        return (rate.Id, rate.PriceCents);
    }

    private static string NormalizePostalCode(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    private static int ToCents(decimal value)
    {
        if (value < 0 || value > int.MaxValue)
            throw new ValidationException("Order total is too large.");
        return (int)value;
    }

    private static (int UnitPriceCents, List<OrderItemOptionSnapshot> Options) PriceItem(
        Product product, Dictionary<int, int>? selectedOptions)
    {
        selectedOptions ??= new Dictionary<int, int>();
        var unitPrice = (long)product.BasePriceCents;
        var snapshot = new List<OrderItemOptionSnapshot>(selectedOptions.Count);

        foreach (var (optionId, choiceId) in selectedOptions)
        {
            var option = product.ProductOptions.FirstOrDefault(o => o.Id == optionId)
                         ?? throw new ValidationException($"Option {optionId} is not valid for {product.Name}.");
            var choice = option.ProductOptionChoices.FirstOrDefault(c => c.Id == choiceId && c.IsActive)
                         ?? throw new ValidationException(
                             $"Choice {choiceId} is not valid for option '{option.Name}'.");

            // TODO: ProductOptionChoice also has SalePriceDeltaCents. Decide when it applies and use it here.
            unitPrice += choice.PriceDeltaCents;

            // Snapshot names and price so later catalog edits don't rewrite order history.
            snapshot.Add(new OrderItemOptionSnapshot(option.Name, choice.Name, choice.PriceDeltaCents));
        }

        var missing = product.ProductOptions.FirstOrDefault(o => o.IsRequired && !selectedOptions.ContainsKey(o.Id));
        if (missing is not null)
            throw new ValidationException($"Option '{missing.Name}' is required for {product.Name}.");

        if (unitPrice <= 0 || unitPrice > int.MaxValue)
            throw new ValidationException($"Invalid price for {product.Name}.");

        return ((int)unitPrice, snapshot);
    }

    private static CreateOrderResult ToResult(Order order) => new(
        Id: order.Id,
        OrderNumber: order.OrderNumber,
        CustomerEmail: order.CustomerEmail,
        CustomerFirstName: order.CustomerFirstName,
        CustomerLastName: order.CustomerLastName,
        Status: order.Status.ToString(),
        PaymentStatus: order.PaymentStatus.ToString(),
        FulfillmentStatus: order.FulfillmentStatus?.ToString(),
        SubtotalCents: order.SubtotalCents,
        DiscountCents: order.DiscountCents,
        TaxCents: order.TaxCents,
        ShippingCents: order.ShippingCents,
        TotalCents: order.TotalCents,
        Currency: order.Currency,
        CreatedAt: order.CreatedAt,
        Items: order.OrderItems.Select(i => new CreateOrderItemResult(
            ProductName: i.ProductName,
            Quantity: i.Quantity,
            UnitPriceCents: i.UnitPriceCents,
            TotalPriceCents: i.TotalPriceCents,
            SelectedOptions: ParseOptions(i.OptionDetails)
                .Select(o => new CreateOrderItemOptionResult(o.OptionName, o.ChoiceName, o.PriceDeltaCents))
                .ToList())).ToList());

    private static string ValidateRequest(CreateOrderRequest request)
    {
        if (request.Items is not { Count: > 0 and <= MaxItems })
            throw new ValidationException($"Order must contain between 1 and {MaxItems} items.");

        foreach (var item in request.Items)
        {
            if (item.Quantity is <= 0 or > MaxQuantity)
                throw new ValidationException($"Item quantity must be between 1 and {MaxQuantity}.");
            if (item.SelectedOptions is { Count: > MaxOptionsPerItem })
                throw new ValidationException("Too many options selected for one item.");
        }

        RequireText(request.CustomerFirstName, "First name", 100);
        RequireText(request.CustomerLastName, "Last name", 100);
        var email = ParseEmail(request.CustomerEmail);

        if (request.CustomerPhone is { Length: > 30 })
            throw new ValidationException("Phone number is too long.");
        if (request.CustomerNotes is { Length: > 2000 })
            throw new ValidationException("Notes must be at most 2000 characters.");
        if (request.IdempotencyKey is { Length: > 64 })
            throw new ValidationException("Idempotency key is too long.");

        ValidateAddress(request.ShippingAddress, "Shipping");
        if (request.BillingAddress is not null) ValidateAddress(request.BillingAddress, "Billing");

        return email;
    }

    private static void ValidateAddress(AddressRequest? address, string label)
    {
        if (address is null) throw new ValidationException($"{label} address is required.");

        RequireText(address.Line1, $"{label} address line 1", 200);
        RequireText(address.City, $"{label} city", 100);
        RequireText(address.PostalCode, $"{label} postal code", 20);
        RequireText(address.Country, $"{label} country", 100);

        if (address.Line2 is { Length: > 200 })
            throw new ValidationException($"{label} address line 2 is too long.");
        if (address.State is { Length: > 100 })
            throw new ValidationException($"{label} state is too long.");
    }

    private static string ParseEmail(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 254 ||
            !MailboxAddress.TryParse(trimmed, out var mailbox) || !mailbox.Address.Contains('@'))
            throw new ValidationException("A valid customer email is required.");

        return mailbox.Address;
    }

    private static void RequireText(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ValidationException($"{field} must not be empty.");
        if (trimmed.Length > maxLength)
            throw new ValidationException($"{field} must be at most {maxLength} characters.");
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
            ShippingRateId: order.ShippingRateId ?? 0,
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
            OldFulfillmentStatus = oldFulfillmentStatus?.ToString() ?? "",
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

    private async Task<Order> PersistAsync(
        int tenantId,
        CreateOrderRequest request,
        string email,
        string? idempotencyKey,
        PricedOrder priced,
        CancellationToken ct)
    {
        var http = httpContextAccessor.HttpContext;
        var ipAddress = http?.Connection.RemoteIpAddress?.ToString(); // needs forwarded headers behind a proxy
        var userAgent = Clean(http?.Request.Headers.UserAgent.ToString());
        if (userAgent is { Length: > 512 }) userAgent = userAgent[..512];

        // A single SaveChanges is already atomic (and retried by the execution strategy),
        // so no explicit transaction is needed until this method does multi-step work
        // (stock decrement, outbox row, payment record).
        for (var attempt = 1;; attempt++)
        {
            var order = BuildOrder(tenantId, request, email, idempotencyKey, priced, ipAddress, userAgent);
            database.Orders.Add(order);

            database.EmailOutbox.Add(new EmailOutboxMessage
            {
                TenantId = tenantId,
                Order = order, // navigation: EF fills in OrderId after the order is inserted
                Type = EmailType
                    .OrderReceived // see notes: usually the receipt is queued by the payment webhook instead
            });
            
            try
            {
                await database.SaveChangesAsync(ct);
                return order;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Discard the failed graph before querying again.
                database.ChangeTracker.Clear();

                // Duplicate submit or an ambiguous-commit retry: hand back the existing order.
                if (idempotencyKey is not null)
                {
                    var existing = await FindByIdempotencyKeyAsync(tenantId, idempotencyKey, ct);
                    if (existing is not null) return existing;
                }

                // Otherwise assume an order number collision and try again with a new number.
                if (attempt >= MaxOrderNumberAttempts) throw;
                logger.LogWarning(ex,
                    "Unique violation creating order for tenant {TenantId}; retrying (attempt {Attempt})",
                    tenantId, attempt);
            }
        }
    }

    private Order BuildOrder(
        int tenantId,
        CreateOrderRequest request,
        string email,
        string? idempotencyKey,
        PricedOrder priced,
        string? ipAddress,
        string? userAgent)
    {
        var shipping = request.ShippingAddress!;
        var billing = request.BillingAddress ?? shipping; // billing defaults to the shipping address

        return new Order
        {
            TenantId = tenantId,
            OrderNumber = GenerateOrderNumber(),
            IdempotencyKey = idempotencyKey,
            ShippingRateId = priced.ShippingRateId,

            CustomerEmail = email,
            CustomerFirstName = request.CustomerFirstName.Trim(),
            CustomerLastName = request.CustomerLastName.Trim(),
            CustomerPhone = Clean(request.CustomerPhone),

            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid, // the payment webhook moves this forward
            FulfillmentStatus = FulfillmentStatus.Unfulfilled,

            SubtotalCents = priced.SubtotalCents,
            DiscountCents = 0,
            CouponCode = null,
            CouponDiscountCents = 0,
            TaxCents = priced.TaxCents,
            ShippingCents = priced.ShippingCents,
            TotalCents = priced.TotalCents,
            RefundedAmountCents = 0,
            PaymentServiceFeeCents = 0,
            Currency = CurrencyCode,

            ShippingAddressLine1 = shipping.Line1.Trim(),
            ShippingAddressLine2 = Clean(shipping.Line2),
            ShippingCity = shipping.City.Trim(),
            ShippingState = Clean(shipping.State),
            ShippingPostalCode = shipping.PostalCode.Trim(),
            ShippingCountry = shipping.Country.Trim(),

            BillingAddressLine1 = billing.Line1.Trim(),
            BillingAddressLine2 = Clean(billing.Line2),
            BillingCity = billing.City.Trim(),
            BillingState = Clean(billing.State),
            BillingPostalCode = billing.PostalCode.Trim(),
            BillingCountry = billing.Country.Trim(),

            OrderType = OrderType.Standard,

            IpAddress = ipAddress,
            UserAgent = userAgent,

            CustomerNotes = Clean(request.CustomerNotes),
            AdminNotes = null,

            OrderItems = priced.Items.Select(i => new OrderItem
            {
                TenantId = tenantId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPriceCents = i.UnitPriceCents,
                TotalPriceCents = i.TotalPriceCents,
                TaxCategoryId = i.TaxCategoryId,
                TaxCents = i.TaxCents,
                WeightGrams = i.WeightGrams,
                OptionDetails = i.Options.Count == 0 ? null : JsonSerializer.Serialize(i.Options),
            }).ToList(),
        };
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<OrderItemOptionSnapshot> ParseOptions(string? json) =>
        string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<OrderItemOptionSnapshot>>(json) ?? [];

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is MySqlException { Number: 1062 };

    private Task<Order?> FindByIdempotencyKeyAsync(int tenantId, string key, CancellationToken ct) =>
        database.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.IdempotencyKey == key, ct);

    private sealed record PricedItem(
        int ProductId,
        string ProductName,
        int Quantity,
        int UnitPriceCents,
        int TotalPriceCents,
        int? TaxCategoryId,
        int TaxCents,
        int? WeightGrams,
        bool RequiresShipping,
        List<OrderItemOptionSnapshot> Options);

    private sealed record PricedOrder(
        List<PricedItem> Items,
        int SubtotalCents,
        int TaxCents,
        int ShippingCents,
        int? ShippingRateId,
        int TotalCents);
}