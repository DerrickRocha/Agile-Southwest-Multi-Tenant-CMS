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

    public async Task<IEnumerable<GetProductImageResult>> GetProductImagesByProductId(int productId)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        // Verify product belongs to tenant
        var product = await database.Products
                          .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenant.Id && p.DeletedAt == null)
                      ?? throw new InvalidOperationException($"Product {productId} not found");

        var productImages = await database.ProductImages
            .AsNoTracking()
            .Include(pi => pi.Image)
            .Where(pi => pi.TenantId == tenant.Id
                         && pi.ProductId == productId
                         && pi.DeletedAt == null)
            .OrderBy(pi => pi.Position)
            .ToListAsync();

        return productImages.Select(pi => new GetProductImageResult(
            pi.Id,
            pi.TenantId,
            pi.ProductId,
            product.Name,
            pi.ImageId,
            pi.Image?.Url ?? string.Empty,
            pi.Image?.OriginalFileName,
            pi.IsPrimary,
            pi.Position,
            pi.CreatedAt,
            pi.UpdatedAt
        ));
    }

    public async Task<IEnumerable<GetProductImageResult>> GetProductImagesByImageId(int imageId)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var productImages = await database.ProductImages
            .AsNoTracking()
            .Include(pi => pi.Image)
            .Include(pi => pi.Product)
            .Where(pi => pi.TenantId == tenant.Id
                         && pi.ImageId == imageId
                         && pi.DeletedAt == null)
            .OrderBy(pi => pi.Position)
            .ToListAsync();

        return productImages.Select(pi => new GetProductImageResult(
            pi.Id,
            pi.TenantId,
            pi.ProductId,
            pi.Product?.Name ?? string.Empty,
            pi.ImageId,
            pi.Image?.Url ?? string.Empty,
            pi.Image?.OriginalFileName,
            pi.IsPrimary,
            pi.Position,
            pi.CreatedAt,
            pi.UpdatedAt
        ));
    }

    public async Task SetAsPrimary(int id)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
            
        var productImage = await database.ProductImages
                               .FirstOrDefaultAsync(pi => pi.Id == id && pi.TenantId == tenant.Id && pi.DeletedAt == null)
                           ?? throw new InvalidOperationException("Product image association not found");
        
        await ClearPrimaryFlag(tenant.Id, productImage.ProductId);

        productImage.IsPrimary = true;
        productImage.UpdatedAt = DateTime.UtcNow;

        await database.SaveChangesAsync();
        
    }

    public async Task UpdatePosition(int id, int? newPosition)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
            
        if (newPosition is null or < 0)
            throw new ValidationException("Position cannot be null or negative");

        var productImage = await database.ProductImages
                               .FirstOrDefaultAsync(pi => pi.Id == id && pi.TenantId == tenant.Id && pi.DeletedAt == null)
                           ?? throw new InvalidOperationException("Product image association not found");

        // Check if position is already taken
        var existingAtPosition = await database.ProductImages
            .FirstOrDefaultAsync(pi => pi.TenantId == tenant.Id 
                                       && pi.ProductId == productImage.ProductId 
                                       && pi.Position == newPosition 
                                       && pi.DeletedAt == null
                                       && pi.Id != id);

        if (existingAtPosition != null)
            throw new ValidationException($"Position {newPosition} is already taken");

        productImage.Position = newPosition?? 0;
        productImage.UpdatedAt = DateTime.UtcNow;

        await database.SaveChangesAsync();
    }

    public async Task ReorderImages(ReorderImagesRequest request)
    {
            var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
            
            // Verify product belongs to tenant
            var product = await database.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.TenantId == tenant.Id && p.DeletedAt == null)
                ?? throw new InvalidOperationException($"Product {request.ProductId} not found");

            // Get all current associations for this product
            var associations = await database.ProductImages
                .Where(pi => pi.TenantId == tenant.Id 
                    && pi.ProductId == request.ProductId 
                    && pi.DeletedAt == null)
                .ToListAsync();

            // Validate all IDs exist
            var associationIds = associations.Select(a => a.Id).ToHashSet();
            var missingIds = request.ImageOrder.Except(associationIds).ToList();
            if (missingIds.Any())
                throw new ValidationException($"Invalid association IDs: {string.Join(", ", missingIds)}");

            // Update positions
            foreach (var association in associations)
            {
                var newPosition = Array.IndexOf(request.ImageOrder, association.Id);
                if (newPosition >= 0 && association.Position != newPosition)
                {
                    association.Position = newPosition;
                    association.UpdatedAt = DateTime.UtcNow;
                }
            }

            // If a primary image is specified, ensure it's the only one
            if (request.PrimaryImageId.HasValue)
            {
                if (!associationIds.Contains(request.PrimaryImageId.Value))
                    throw new ValidationException($"Primary image ID {request.PrimaryImageId} not found in product images");

                foreach (var association in associations)
                {
                    association.IsPrimary = (association.Id == request.PrimaryImageId.Value);
                }
            }

            await database.SaveChangesAsync();
            
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