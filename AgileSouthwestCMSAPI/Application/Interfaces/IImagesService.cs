using AgileSouthwestCMSAPI.Application.DTOs.Images;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IImagesService
{
    public Task<AddImageResult> AddImage(IFormFile file);

    public Task<GetImageResult> GetImage(int id);
}