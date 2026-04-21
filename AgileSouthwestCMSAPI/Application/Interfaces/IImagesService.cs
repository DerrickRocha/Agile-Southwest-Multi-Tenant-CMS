using AgileSouthwestCMSAPI.Api.Requests.Images;
using AgileSouthwestCMSAPI.Application.DTOs.Images;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IImagesService
{
    public Task<ImageResult> AddImages(AddImagesRequest request);
}