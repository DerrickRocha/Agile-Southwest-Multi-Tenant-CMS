using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Extensions;

public static class ProductRequestExtensions
{
    extension(ProductRequest request)
    {
        public ProductOption[] ToProductOptions()
        {
            var options = request.Options.Select(o =>
            
                new ProductOption
                {
                    Name = o.Name,
                    IsRequired = o.IsRequired?? false,
                    ProductOptionChoices = o.Choices.Select(c => new ProductOptionChoice
                    {
                        Name = c.Name,
                        PriceDeltaCents = c.PriceDelta,
                        SalePriceDeltaCents = c.SalePriceDelta,
                        IsActive = c.IsActive?? false
                    }).ToArray()
                }
            );
            return options.ToArray();
        }
        
        public Product ToProduct(int tenantId)
        {
            var options = request.Options.Select(option =>
                {
                    var choices = option.Choices.Select(optionChoice => new ProductOptionChoice
                    {
                        Name = optionChoice.Name,
                        PriceDeltaCents = optionChoice.PriceDelta,
                        SalePriceDeltaCents = optionChoice.SalePriceDelta,
                        IsActive = optionChoice.IsActive ?? false
                    });
                    var productOption = new ProductOption
                    {
                        Name = option.Name,
                        IsRequired = option.IsRequired ?? false,
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
                IsActive = request.IsActive ?? false,
                ProductOptions = options
            };
            return product;
        }
    }
}