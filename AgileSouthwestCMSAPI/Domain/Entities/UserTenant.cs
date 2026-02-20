namespace AgileSouthwestCMSAPI.Domain.Entities;

public class UserTenant
{
    public Guid UserId { get; set; }
    public CmsUser User { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }

    public string Role { get; set; } 
}