using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Products;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IProductsService
{
    public Task<ProductResult> CreateProduct(CreateProductRequest request);
    public Task<ProductResult> GetProduct(int id);
    public Task<ProductResult> UpdateProduct(int id, ProductRequest request);
    public Task<ProductResult> DeleteProduct();
    public Task<IEnumerable<ProductResult>> GetProducts();
}