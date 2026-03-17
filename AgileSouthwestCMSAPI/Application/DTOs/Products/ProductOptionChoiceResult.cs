namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class ProductOptionChoiceResult
{
    public int Id { get; set; }
    public int ProductOptionId { get; set; }
    public string Name { get; set; }
    public int PriceDelta { get; set; }
    public int SalePriceDelta { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}