namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class CreateProductOptionRequest
{
    public string Name { set; get;}
    
    public bool IsRequired { set; get;} = true;
    public CreateProductOptionChoice[] ProductOptionChoices { set; get;}
}