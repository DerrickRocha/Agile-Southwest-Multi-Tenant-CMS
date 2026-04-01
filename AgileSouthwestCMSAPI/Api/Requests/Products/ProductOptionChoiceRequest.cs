
namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public class ProductOptionChoiceRequest
{
    public string? Name { set; get;}
    public int? PriceDelta { set; get;}
    
    public int? SalePriceDelta { set; get;}

    public bool? IsActive { set; get; }
}