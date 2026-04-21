using AgileSouthwestCMSAPI.Api.Requests.Images;
using AgileSouthwestCMSAPI.Application.DTOs.Images;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ImagesService: IImagesService
{
    public Task<ImageResult> AddImages(AddImagesRequest request)
    {
        return Task.FromResult(new ImageResult(1));
    }
}