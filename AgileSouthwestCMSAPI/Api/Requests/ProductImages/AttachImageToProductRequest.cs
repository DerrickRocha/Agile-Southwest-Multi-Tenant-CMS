using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.ProductImages;

public record AttachImageToProductRequest(
    int ProductId,
    int ImageId,
    bool IsPrimary = false,
    int? Position = null
);