using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductsService(ITenantContext context, CmsDbContext database) : IProductsService
{
    public async Task<ProductResult> CreateProduct(CreateProductRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var strategy = database.Database.CreateExecutionStrategy();

        if (request.ProductOptions.Length == 0)
        {
            if(request.BasePrice <= 0) throw new InvalidOperationException("Base price must be greater than 0");

        }

        var hasNoValue = request.ProductOptions.Any(o => o.ProductOptionChoices.Any(c => c.PriceDelta == 0));
        if (hasNoValue)
        {
            if(request.BasePrice <= 0) throw new InvalidOperationException("Base price must be greater than 0");
        }

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.Database.BeginTransactionAsync();

            try
            {
                var options = request.ProductOptions.Select(option =>
                    {
                        var choices = option.ProductOptionChoices.Select(optionChoice => new ProductOptionChoice
                        {
                            Name = optionChoice.Name,
                            PriceDeltaCents = optionChoice.PriceDelta,
                            SalePriceDeltaCents = optionChoice.SalePriceDelta,
                        });
                        var productOption = new ProductOption
                        {
                            Name = option.Name,
                            ProductOptionChoices = choices.ToList()
                        };
                        return productOption;
                    }
                ).ToList();
                
                var product = new Product
                {
                    TenantId = tenant.Id,
                    Name = request.Name,
                    Description = request.Description,
                    BasePriceCents = request.BasePrice,
                    IsActive = request.IsActive,
                    ProductOptions = options
                };
                database.Products.Add(product);
                await database.SaveChangesAsync();
                await transaction.CommitAsync();
                return ToProductResult(product);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private ProductResult ToProductResult(Product product)
    {
        var options = product.ProductOptions.Select(option => new ProductOptionResult
        {
            Id = option.Id,
            ProductId = option.ProductId,
            Name = option.Name,
            CreatedAt = option.CreatedAt,
            UpdatedAt = option.UpdatedAt,
            ProductOptionChoices = option.ProductOptionChoices.Select(choice => new ProductOptionChoiceResult
            {
                Id = choice.Id,
                ProductOptionId = choice.ProductOptionId,
                Name = choice.Name,
                PriceDelta = choice.PriceDeltaCents,
                SalePriceDelta = choice.SalePriceDeltaCents,
                CreatedAt = choice.CreatedAt,
                UpdatedAt = choice.UpdatedAt
            }).ToArray()
        }).ToArray();
        return new ProductResult
        {
            Id = product.Id,
            TenantId = product.TenantId,
            Name = product.Name,
            Description = product.Description,
            BasePrice = product.BasePriceCents,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            ProductOptions = options
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