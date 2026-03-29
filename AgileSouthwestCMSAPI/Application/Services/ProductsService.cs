using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Extensions;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductsService(ITenantContext context, CmsDbContext database, bool skipTransactionsForTesting = false) : IProductsService
{
    public async Task<ProductResult> CreateProduct(ProductRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var strategy = database.Database.CreateExecutionStrategy();

        CheckFields(request);

        if (skipTransactionsForTesting) return await WriteProduct(request, tenant.Id);
        
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.Database.BeginTransactionAsync();

            try
            {

                var result = await WriteProduct(request, tenant.Id);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task<ProductResult> WriteProduct(ProductRequest request, int tenantId)
    {
        var product = request.ToProduct(tenantId);
        database.Products.Add(product);
        await database.SaveChangesAsync();
        return product.ToProductResult();
    }

    public async Task<ProductResult> GetProduct(int id)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        
        var product = await database.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenant.Id);
        return product == null ? throw new InvalidOperationException("Product not found.") : product.ToProductResult();
    }

    public async Task<ProductResult> UpdateProduct(int id, ProductRequest request)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var product = await database.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenant.Id);
        if (product == null) throw new InvalidOperationException("Product not found.");
        CheckFields(request);
        product.Name = request.Name;
        product.Description = request.Description;
        product.BasePriceCents = request.BasePrice;
        product.IsActive = request.IsActive?? false;
        product.ProductOptions = request.ToProductOptions();
        
        await database.SaveChangesAsync();
        
        return product.ToProductResult();
    }

    private void CheckFields(ProductRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("Name is required.");

        if (request.IsActive == null)
        {
            throw new InvalidOperationException("Is Active is required.");
        }
        
        var optionNameRequired = request.Options.Any(o => string.IsNullOrWhiteSpace(o.Name));
        if (optionNameRequired) throw new InvalidOperationException("Option name is required.");
        
        var optionChoiceNameRequired = request.Options.Any(o => o.Choices.Any(c => string.IsNullOrWhiteSpace(c.Name)));
        if (optionChoiceNameRequired) throw new InvalidOperationException("Option choice name is required.");
        
        var requiresBasePrice = request.Options.Any(o => o.Choices.Any(c => c.PriceDelta == 0));
        if (requiresBasePrice && request.BasePrice <= 0) throw new InvalidOperationException("Base price must be greater than 0");
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