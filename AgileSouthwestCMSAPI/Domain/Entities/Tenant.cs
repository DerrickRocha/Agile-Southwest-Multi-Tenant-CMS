using System.ComponentModel.DataAnnotations.Schema;
using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

[Table( "tenants")]
public class Tenant
{
    [Column( "id" )]
    public int Id { set; get; }
    
    [Column( "name" )]
    public string Name { set; get; }
    
    [Column( "sub_domain" )]
    public required string SubDomain { set; get; }
    
    [Column( "custom_domain" )]
    public string? CustomDomain { set; get; }
    
    [Column( "plan_tier" )]
    public PlanTier PlanTier { set; get; }
    
    [Column( "subscription_status" )]
    public SubscriptionStatus SubscriptionStatus { set; get; }
    
    [Column( "status" )]
    public TenantStatus Status { set; get; }
    
    [Column( "created_at" )]
    public DateTime CreatedAt { get; set; }
    
    [Column( "updated_at" )]
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();}