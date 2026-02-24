namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class AddTenantResult
{
    public int TenantId { get; set; }
    public string SubDomain { get; set; }
    public string CustomDomain { get; set; }
    public string Name { get; set; }
    public string PlanTier { get; set; }
    public string Status { get; set; }
    public string SubscriptionStatus { get; set; }
}