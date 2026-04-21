using AgileSouthwestCMSAPI.Application.DTOs.Images;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IImagesService
{
    public Task<ImageResult> AddImages(IFormFile file);
}