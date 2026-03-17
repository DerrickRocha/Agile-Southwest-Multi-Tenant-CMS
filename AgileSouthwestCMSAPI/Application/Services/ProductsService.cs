using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductsService(ITenantContext context, CmsDbContext database): IProductsService
{
    public async Task<CreateProductResult> CreateProduct(CreateProductRequest request)
    {
        return new CreateProductResult
        {
            Id = 1
        };
    }
}