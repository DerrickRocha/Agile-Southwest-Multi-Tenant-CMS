using AgileSouthwestCMSAPI.Application.DTOs.Images;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ImagesService: IImagesService
{
    public Task<ImageResult> AddImages(IFormFile file)
    {
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
        
        return Task.FromResult(new ImageResult(1));
    }
}