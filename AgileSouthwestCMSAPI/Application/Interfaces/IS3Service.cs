using AgileSouthwestCMSAPI.Application.DTOs.S3;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IS3Service
{
    public Task<S3ImageResult> UploadImage();
}