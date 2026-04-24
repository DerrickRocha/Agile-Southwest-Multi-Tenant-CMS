using AgileSouthwestCMSAPI.Api.Requests.ProductImages;
using AgileSouthwestCMSAPI.Application.DTOs.ProductImages;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IProductImagesService
{
    Task<AttachImageResult> AttachImageToProduct(AttachImageToProductRequest request);
    Task<GetProductImageResult> GetProductImage(int id);
    Task<IEnumerable<GetProductImageResult>> GetProductImagesByProductId(int productId);
    Task<IEnumerable<GetProductImageResult>> GetProductImagesByImageId(int imageId);
    Task SetAsPrimary(int id);
    Task UpdatePosition(int id, int newPosition);
    Task ReorderImages(ReorderImagesRequest request);
    Task DetachImageFromProduct(int id);
    Task DetachImageFromProductByKeys(int productId, int imageId);
}