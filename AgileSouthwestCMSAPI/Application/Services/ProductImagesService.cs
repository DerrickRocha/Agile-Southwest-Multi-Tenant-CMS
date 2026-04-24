using AgileSouthwestCMSAPI.Api.Requests.ProductImages;
using AgileSouthwestCMSAPI.Application.DTOs.ProductImages;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductImagesService: IProductImagesService
{
    public Task<AttachImageResult> AttachImageToProduct(AttachImageToProductRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<GetProductImageResult> GetProductImage(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GetProductImageResult>> GetProductImagesByProductId(int productId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GetProductImageResult>> GetProductImagesByImageId(int imageId)
    {
        throw new NotImplementedException();
    }

    public Task SetAsPrimary(int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePosition(int id, int newPosition)
    {
        throw new NotImplementedException();
    }

    public Task ReorderImages(ReorderImagesRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DetachImageFromProduct(int id)
    {
        throw new NotImplementedException();
    }

    public Task DetachImageFromProductByKeys(int productId, int imageId)
    {
        throw new NotImplementedException();
    }
}