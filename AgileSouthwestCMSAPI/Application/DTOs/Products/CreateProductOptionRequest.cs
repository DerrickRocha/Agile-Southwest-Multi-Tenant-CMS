namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class CreateProductOptionRequest
{
    public string Name { set; get;}
    public CreateProductOptionChoice[] ProductOptionChoices { set; get;}
}