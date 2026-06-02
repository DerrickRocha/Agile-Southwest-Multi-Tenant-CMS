
namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public class ProductRequest
{
    public string? Name { set; get;}
    public string? Description { set; get;}
    public int? BasePrice { set; get;}
    
    public bool? IsActive { set; get; }
    public int TaxCategoryId { set; get; }

    public ProductOptionRequest[] Options { set; get; }
}