using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Tenant
{
    public Guid TenantId { set; get; }
    
    public string Name { set; get; }
    
    public string SubDomain { set; get; }
    
    public string? CustomDomain { set; get; }
    
    public PlanTier PlanTier { set; get; }
    
    public SubscriptionStatus SubscriptionStatus { set; get; }
    
    public TenantStatus Status { set; get; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public ICollection<CmsUser> Users { get; set; } = new List<CmsUser>();
}