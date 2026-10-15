
namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record CreateOrderResult(
    int Id,
    string OrderNumber,
    string CustomerEmail,
    string CustomerFirstName,
    string CustomerLastName,
    string Status,
    string PaymentStatus,
    string? FulfillmentStatus,
    int SubtotalCents,
    int DiscountCents,
    int TaxCents,
    int ShippingCents,
    int TotalCents,
    string Currency,
    DateTime CreatedAt,
    IReadOnlyList<CreateOrderItemResult> Items);