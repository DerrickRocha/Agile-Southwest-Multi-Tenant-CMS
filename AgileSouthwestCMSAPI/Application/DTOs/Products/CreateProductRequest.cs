namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class CreateProductRequest
{
    public string Name { set; get;}
    public string Description { set; get;}
    public int BasePrice { set; get;}
    public bool IsActive { set; get;}
}