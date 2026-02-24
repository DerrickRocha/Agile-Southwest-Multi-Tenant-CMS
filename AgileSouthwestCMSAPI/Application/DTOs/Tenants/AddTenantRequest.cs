namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class AddTenantRequest
{
    public string Name {set; get;}
    public string SubDomain {set; get;}
    public string? CustomDomain {set; get;}
}