using System.ComponentModel.DataAnnotations.Schema;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ProductOptionChoice
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("tenant_id")]
    public int TenantId { get; set; }
    
    [Column("option_id")]
    public int ProductOptionId { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("price_delta_cents")]
    public int PriceDeltaCents { get; set; }
    
    [Column("sale_price_delta_cents")]
    public int? SalePriceDeltaCents { get; set; }
    
    [Column("is_active")]
    public bool IsActive { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }
    public ProductOption ProductOption { get; set; } = null!;
    
    public Tenant? Tenant { get; set; }
}