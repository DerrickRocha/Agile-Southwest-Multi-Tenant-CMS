using AgileSouthwestCMSAPI.Application.DTOs.S3;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class S3Service: IS3Service
{
    public async Task<S3ImageResult> UploadImage()
    {
        return new S3ImageResult("https://www.test.com");
    }
}