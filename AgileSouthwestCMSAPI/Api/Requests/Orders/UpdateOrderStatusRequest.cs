using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record UpdateOrderStatusRequest(
    [Required] string NewStatus,
    string? Reason,
    string? TrackingNumber,
    string? TrackingUrl
);