namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public class UpdateProductOptionRequest
{
    public string Name { set; get;}
    
    public bool IsRequired { set; get;} = true;
    
    public UpdateProductOptionChoiceRequest[] Choices { set; get;}
}