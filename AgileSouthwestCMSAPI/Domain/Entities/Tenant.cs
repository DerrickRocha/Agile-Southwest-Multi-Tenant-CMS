using System.ComponentModel.DataAnnotations.Schema;

namespace AgileSouthwestCMSAPI.Domain.Entities;

[Table( "tenants")]
public class Tenant
{
    [Column( "id")]
    public int Id { set; get; }
    
    [Column( "name" )]
    public string Name { set; get; }
    
    [Column( "sub_domain" )]
    public required string SubDomain { set; get; }
    
    [Column( "custom_domain" )]
    public string? CustomDomain { set; get; }
    
    [Column( "created_at" )]
    public DateTime CreatedAt { get; set; }
    
    [Column( "updated_at" )]
    public DateTime UpdatedAt { get; set; }
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    public DateTime RowVersion { get; set; }
    
    // Navigation
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Store> Stores { get; set; } = new List<Store>();

    public ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();

    public ICollection<Image> Images { get; set; } = new List<Image>();
}
    
    