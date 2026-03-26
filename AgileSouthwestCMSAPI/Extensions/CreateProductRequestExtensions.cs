using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Extensions;

public static class CreateProductRequestExtensions
{
    extension(CreateProductRequest request)
    {
        public Product ToProduct(int tenantId)
        {
            var options = request.ProductOptions.Select(option =>
                {
                    var choices = option.ProductOptionChoices.Select(optionChoice => new ProductOptionChoice
                    {
                        Name = optionChoice.Name,
                        PriceDeltaCents = optionChoice.PriceDelta,
                        SalePriceDeltaCents = optionChoice.SalePriceDelta,
                        IsActive = optionChoice.IsActive
                    });
                    var productOption = new ProductOption
                    {
                        Name = option.Name,
                        IsRequired = option.IsRequired,
                        ProductOptionChoices = choices.ToList()
                    };
                    return productOption;
                }
            ).ToList();
                
            var product = new Product
            {
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                BasePriceCents = request.BasePrice,
                IsActive = request.IsActive,
                ProductOptions = options
            };
            return product;
        }
    }
}