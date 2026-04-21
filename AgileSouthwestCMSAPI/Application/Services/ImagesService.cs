using AgileSouthwestCMSAPI.Application.DTOs.Images;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ImagesService: IImagesService
{
    public Task<ImageResult> AddImages(IFormFileCollection files)
    {
        return Task.FromResult(new ImageResult(1));
    }
}