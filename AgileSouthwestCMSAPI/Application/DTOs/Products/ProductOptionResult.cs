namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class ProductOptionResult
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ProductOptionChoiceResult[] ProductOptionChoices { get; set; }

}