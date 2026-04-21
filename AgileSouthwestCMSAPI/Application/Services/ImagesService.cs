using AgileSouthwestCMSAPI.Application.DTOs.Images;
using AgileSouthwestCMSAPI.Application.DTOs.S3;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ImagesService(IS3Service s3Service): IImagesService
{
    public async Task<ImageResult> AddImage(IFormFile file)
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
        
        // 4. Validate magic bytes (defeats file renaming attacks)
        using var stream = file.OpenReadStream();
        if (!IsValidImageSignature(stream, extension))
            throw new BadHttpRequestException("File content does not match its extension");
        
        var s3Result = await s3Service.UploadImage();
        var imageResult = AddImageToDatabase(s3Result);
        
        return imageResult;
    }

    private ImageResult AddImageToDatabase(S3ImageResult s3Result)
    {
        return new ImageResult(1);
    }

    private bool IsValidImageSignature(Stream stream, string extension)
    {
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var header = reader.ReadBytes(8);
        stream.Position = 0;
    
        return extension switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".gif" => header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46,
            ".webp" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46,
            _ => false
        };
    }
}