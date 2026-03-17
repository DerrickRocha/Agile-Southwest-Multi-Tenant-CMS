namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public class ProductResult
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int BasePrice { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}