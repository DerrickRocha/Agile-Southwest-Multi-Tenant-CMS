using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductsService(ITenantContext context, CmsDbContext database): IProductsService
{
    public async Task<ProductResult> CreateProduct(CreateProductRequest request)
    {
        return new ProductResult()
        {
            Id = 1
        };
    }

    public async Task<ProductResult> GetProduct(int id)
    {
        return new ProductResult();
    }

    public async Task<ProductResult> UpdateProduct(UpdateProductRequest request)
    {
      return new ProductResult();
    }
    
    public async Task<ProductResult> DeleteProduct()
    {
      return new ProductResult();
    }
    
    public async Task<IEnumerable<ProductResult>> GetProducts()
    {
      return new List<ProductResult>();
    }
}