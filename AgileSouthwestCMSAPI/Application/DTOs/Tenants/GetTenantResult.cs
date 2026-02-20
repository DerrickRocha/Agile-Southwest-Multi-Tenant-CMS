namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class GetTenantResult
{
    public string TenantId { set; get; }
    
    public string Name { set; get; }
    
    public string SubDomain { set; get; }
    
    public string? CustomDomain { set; get; }
    
    public string PlanTier { set; get; }
}