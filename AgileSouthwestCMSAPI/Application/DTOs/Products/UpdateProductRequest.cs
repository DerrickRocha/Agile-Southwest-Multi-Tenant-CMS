namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class UpdateProductRequest
{
    public int Id { set; get;}
    public string Name { set; get;}
    public string Description { set; get;}
    public int BasePrice { set; get;}
    public bool IsActive { set; get;}
}