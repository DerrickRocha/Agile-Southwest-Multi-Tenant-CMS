using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class GetTenantResult
{
    public int TenantId { set; get; }
    
    public string Name { set; get; }
    
    public string SubDomain { set; get; }

    public string CustomDomain { set; get; } = "";
    
    public byte[]? RowVersion { get; set; }
}