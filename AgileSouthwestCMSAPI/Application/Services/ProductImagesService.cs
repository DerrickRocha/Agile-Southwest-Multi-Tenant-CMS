using System.ComponentModel.DataAnnotations;
using AgileSouthwestCMSAPI.Api.Requests.ProductImages;
using AgileSouthwestCMSAPI.Application.DTOs.ProductImages;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductImagesService(
    CmsDbContext database,
    ITenantContext context) : IProductImagesService
{
    public async Task<AttachImageResult> AttachImageToProduct(AttachImageToProductRequest request)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var product = await database.Products
                          .FirstOrDefaultAsync(p =>
                              p.Id == request.ProductId && p.TenantId == tenant.Id && p.DeletedAt == null)
                      ?? throw new InvalidOperationException($"Product {request.ProductId} not found");

        var image = await database.Images
                        .FirstOrDefaultAsync(i =>
                            i.Id == request.ImageId && i.TenantId == tenant.Id && i.DeletedAt == null)
                    ?? throw new InvalidOperationException($"Image {request.ImageId} not found");

        var existing = await database.ProductImages
            .FirstOrDefaultAsync(pi => pi.TenantId == tenant.Id
                                       && pi.ProductId == request.ProductId
                                       && pi.ImageId == request.ImageId
                                       && pi.DeletedAt == null);

        if (existing != null)
            throw new ValidationException("Image is already attached to this product");

        var position = request.Position ?? await GetNextPosition(tenant.Id, request.ProductId);

        var productImage = new ProductImage
        {
            TenantId = tenant.Id,
            ProductId = request.ProductId,
            ImageId = request.ImageId,
            IsPrimary = request.IsPrimary,
            Position = position,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (productImage.IsPrimary)
        {
            await ClearPrimaryFlag(tenant.Id, request.ProductId);
        }

        database.ProductImages.Add(productImage);
        await database.SaveChangesAsync();

        return new AttachImageResult(productImage.Id);
    }

    public async Task<GetProductImageResult> GetProductImage(int id)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var productImage = await database.ProductImages
                               .Include(productImage => productImage.Product)
                               .Include(productImage => productImage.Image)
                               .FirstOrDefaultAsync(pi =>
                                   pi.Id == id && pi.TenantId == tenant.Id && pi.DeletedAt == null)
                           ?? throw new InvalidOperationException("Product image association not found");

        return new GetProductImageResult(
            productImage.Id,
            productImage.TenantId,
            productImage.ProductId,
            productImage.Product?.Name ?? string.Empty,
            productImage.ImageId,
            productImage.Image?.Url ?? string.Empty,
            productImage.Image?.OriginalFileName,
            productImage.IsPrimary,
            productImage.Position,
            productImage.CreatedAt,
            productImage.UpdatedAt
        );
    }

    public Task<IEnumerable<GetProductImageResult>> GetProductImagesByProductId(int productId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GetProductImageResult>> GetProductImagesByImageId(int imageId)
    {
        throw new NotImplementedException();
    }

    public Task SetAsPrimary(int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePosition(int id, int newPosition)
    {
        throw new NotImplementedException();
    }

    public Task ReorderImages(ReorderImagesRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DetachImageFromProduct(int id)
    {
        throw new NotImplementedException();
    }

    public Task DetachImageFromProductByKeys(int productId, int imageId)
    {
        throw new NotImplementedException();
    }

    private async Task<int> GetNextPosition(int tenantId, int productId)
    {
        var maxPosition = await database.ProductImages
            .Where(pi => pi.TenantId == tenantId
                         && pi.ProductId == productId
                         && pi.DeletedAt == null)
            .MaxAsync(pi => (int?)pi.Position) ?? -1;

        return maxPosition + 1;
    }

    private async Task ClearPrimaryFlag(int tenantId, int productId)
    {
        var primaryImages = await database.ProductImages
            .Where(pi => pi.TenantId == tenantId
                         && pi.ProductId == productId
                         && pi.IsPrimary == true
                         && pi.DeletedAt == null)
            .ToListAsync();

        foreach (var pi in primaryImages)
        {
            pi.IsPrimary = false;
            pi.UpdatedAt = DateTime.UtcNow;
        }
    }
}