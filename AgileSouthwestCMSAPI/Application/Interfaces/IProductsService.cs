using AgileSouthwestCMSAPI.Application.DTOs.Products;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IProductsService
{
    public Task<CreateProductResult> CreateProduct(CreateProductRequest request);
}