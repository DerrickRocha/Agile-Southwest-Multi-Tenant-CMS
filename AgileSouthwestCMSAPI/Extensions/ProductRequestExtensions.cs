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
    }
}