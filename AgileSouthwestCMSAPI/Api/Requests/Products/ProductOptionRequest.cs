using System.ComponentModel.DataAnnotations;

namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public class ProductOptionRequest
{
    public string Name { set; get;}
    
    [Required]
    public bool? IsRequired { set; get;}
    
    public ProductOptionChoiceRequest[] Choices { set; get;}
}