namespace AgileSouthwestCMSAPI.Application.DTOs.ProductImages;

public record GetProductImageResult(
    int Id,
    int TenantId,
    int ProductId,
    string ProductName,
    int ImageId,
    string ImageUrl,
    string? OriginalFileName,
    bool IsPrimary,
    int Position,
    DateTime CreatedAt,
    DateTime UpdatedAt
);