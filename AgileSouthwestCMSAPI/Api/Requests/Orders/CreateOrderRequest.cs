using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record CreateOrderRequest(
    [Required] [EmailAddress] string CustomerEmail,
    [Required] string CustomerFirstName,
    [Required] string CustomerLastName,
    string? CustomerPhone,
    [Required] List<OrderRequestItem> Items,
    [Required] AddressRequest ShippingAddress,
    AddressRequest? BillingAddress,
    int? ShippingRateId,
    [StringLength(64)] string? IdempotencyKey,   // new: client-generated GUID per checkout attempt
    string? CustomerNotes
);