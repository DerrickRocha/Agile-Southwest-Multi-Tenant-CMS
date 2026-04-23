using AgileSouthwestCMSAPI.Application.DTOs.S3;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Infrastructure.Configuration;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;

namespace AgileSouthwestCMSAPI.Application.Services;

public class S3Service(IAmazonS3 s3, IOptions<S3Settings> settings): IS3Service
{
    public async Task<S3ImageResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0) throw new BadHttpRequestException("No file uploaded.");
        try
        {
            await using var fileStream = file.OpenReadStream();  // Direct stream
            
            var key = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = key,
                BucketName = settings.Value.BucketName,
                ContentType = file.ContentType,
                AutoCloseStream = true,
                CannedACL = S3CannedACL.PublicRead
            };

            var fileTransferUtility = new TransferUtility(s3);
            await fileTransferUtility.UploadAsync(uploadRequest);
            var region = s3.Config.RegionEndpoint?.SystemName ?? "us-east-1";
            var imageUrl = $"https://{settings.Value.BucketName}.s3.{region}.amazonaws.com/{key}";
            return new S3ImageResult(imageUrl, file.FileName, file.Length, file.ContentType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}