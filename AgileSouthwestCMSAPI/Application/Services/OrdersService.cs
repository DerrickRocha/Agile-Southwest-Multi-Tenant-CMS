using System.ComponentModel.DataAnnotations;
using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.DTOs.Orders;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class OrdersService(ITenantContext context, CmsDbContext database) : IOrderService
{
    public async Task<CreateOrderResult> CreateOrder(CreateOrderRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        if (request.BillingAddress is null) throw new ValidationException("Billing address cannot be null");
        if (request.PaymentMethod is null) throw new ValidationException("Payment method cannot be null");
        if (request.ShippingAddress is null) throw new ValidationException("Shipping address cannot be null");
        if (request.Items.Count <= 0) throw new ValidationException("Item count must be positive");

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
                ShippingAddressLine1 = request.ShippingAddress.Line1,
                ShippingAddressLine2 = request.ShippingAddress.Line2,
                ShippingCity = request.ShippingAddress.City,
                ShippingState = request.ShippingAddress.State,
                ShippingPostalCode = request.ShippingAddress.PostalCode,
                ShippingCountry = request.ShippingAddress.Country,
                BillingAddressLine1 = request.BillingAddress.Line1,
                BillingAddressLine2 = request.BillingAddress.Line2,
                BillingCity = request.BillingAddress.City,
                BillingState = request.BillingAddress.State,
                BillingPostalCode = request.BillingAddress.PostalCode,
                BillingCountry = request.BillingAddress.Country,
                OrderType = OrderType.Standard,
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

    private string GenerateOrderNumber()
    {
        throw new NotImplementedException();
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