using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Extensions;

public static class ProductExtensions
{
    extension(Product product)
    {
        public ProductResult ToProductResult()
        {
            var options = product.ProductOptions.Select(option => new ProductOptionResult
            {
                Id = option.Id,
                ProductId = option.ProductId,
                Name = option.Name,
                CreatedAt = option.CreatedAt,
                UpdatedAt = option.UpdatedAt,
                IsRequired = option.IsRequired,
                ProductOptionChoices = option.ProductOptionChoices.Select(choice => new ProductOptionChoiceResult
                {
                    Id = choice.Id,
                    ProductOptionId = choice.ProductOptionId,
                    Name = choice.Name,
                    PriceDelta = choice.PriceDeltaCents,
                    SalePriceDelta = choice.SalePriceDeltaCents ?? 0,
                    IsActive = choice.IsActive,
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
    }
}