using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record RefundRequest(
    [Required] [Range(1, int.MaxValue)] int AmountCents,
    [Required] string Reason
);