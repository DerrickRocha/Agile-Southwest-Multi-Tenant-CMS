using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record CancelOrderRequest(
    [Required] string Reason
);