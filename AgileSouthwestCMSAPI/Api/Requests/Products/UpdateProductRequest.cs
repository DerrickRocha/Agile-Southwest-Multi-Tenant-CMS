namespace AgileSouthwestCMSAPI.Api.Requests.Products;

public class UpdateProductRequest
{
    public string Name { set; get;}
    public string Description { set; get;}
    public int BasePrice { set; get;}
    public bool IsActive { set; get;}
    
    public UpdateProductOptionRequest[] Options { set; get;}
}