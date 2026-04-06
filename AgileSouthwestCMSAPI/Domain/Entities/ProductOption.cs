using System.ComponentModel.DataAnnotations.Schema;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class ProductOption
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("product_id")]
    public int ProductId { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column( "created_at" )]
    public DateTime CreatedAt { get; set; }
    
    [Column( "updated_at" )]
    public DateTime UpdatedAt { get; set; }

    [Column("is_required")] 
    public bool IsRequired { get; set; } = true;
    
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    public Product Product { get; set; } = null!;
    
    public ICollection<ProductOptionChoice> ProductOptionChoices { get; set; } = new List<ProductOptionChoice>();
}