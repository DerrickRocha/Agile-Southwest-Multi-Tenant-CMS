using AgileSouthwestCMSAPI.Application.DTOs.Images;
using AgileSouthwestCMSAPI.Application.DTOs.S3;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ImagesService(
    IS3Service s3Service,
    CmsDbContext database,
    ITenantContext context
) : IImagesService
{
    public async Task<AddImageResult> AddImage(IFormFile file)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        if (file == null || file.Length == 0)
            throw new BadHttpRequestException("No file uploaded");

        // 2. Validate file type (MIME + extension)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new BadHttpRequestException("Invalid file type. Allowed: JPG, PNG, WEBP, GIF");

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new BadHttpRequestException("Invalid MIME type");

        // 3. Validate file size (e.g., 5 MB max)
        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
            throw new BadHttpRequestException($"File size exceeds {maxSize / 1024 / 1024}MB limit");

        var s3Result = await s3Service.UploadImage(file);
        var imageResult = await AddImageToDatabase(s3Result, tenant.Id);

        return imageResult;
    }

    public async Task<GetImageResult> GetImage(int id)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var image = await database.Images
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenant.Id && i.DeletedAt == null) ??
                    throw new InvalidOperationException("Image not found");
        return new GetImageResult(image.Id, image.TenantId, image.Url, image.OriginalFileName, image.ContentType, image.FileSize);
    }

    private async Task<AddImageResult> AddImageToDatabase(S3ImageResult s3Result, int tenantId)
    {
        var image = new Image
        {
            TenantId = tenantId,
            Url = s3Result.Url,
            OriginalFileName = s3Result.FileName,
            ContentType = s3Result.ContentType,
            FileSize = s3Result.FileSize
        };
        database.Images.Add(image);
        await database.SaveChangesAsync();
        return new AddImageResult(image.Id);
    }
}