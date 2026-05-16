using System.ComponentModel.DataAnnotations.Schema;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Product
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("tenant_id")]
    public int TenantId { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("base_price_cents")]
    public int BasePriceCents { get; set; }
    
    [Column("tax_category_id")]
    public int TaxCategoryId { get; set; }
    
    [Column("is_active")]
    public bool IsActive { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    
    public DateTime RowVersion { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public TaxCategory TaxCategory { get; set; } = null!;
    
    public ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    
    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();

}