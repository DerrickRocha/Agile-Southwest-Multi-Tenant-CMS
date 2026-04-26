using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record AddressRequest(
    [Required] string Line1,
    string? Line2,
    [Required] string City,
    string? State,
    [Required] string PostalCode,
    [Required] string Country
);