using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Products;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IProductsService
{
    public Task<ProductResult> CreateProduct(ProductRequest request);
    public Task<ProductResult> GetProduct(int id);
    public Task<ProductResult> UpdateProduct(int id, ProductRequest request);
    public Task<ProductResult> DeleteProduct();
    public Task<PagedResult<ProductListItemResult>> GetProducts(int page = 1, int pageSize = 20);
    Task<ProductResult> PatchProduct(int id, PatchProductRequest request);
}