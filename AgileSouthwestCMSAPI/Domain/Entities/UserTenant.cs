using System.ComponentModel.DataAnnotations.Schema;

namespace AgileSouthwestCMSAPI.Domain.Entities;

[Table( "user_tenants")]
public class UserTenant
{
    [Column("user_id")]
    public int UserId { get; set; }
    public CmsUser User { get; set; }

    [Column("tenant_id")]
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }

    [Column("role")]
    public string Role { get; set; } 
}