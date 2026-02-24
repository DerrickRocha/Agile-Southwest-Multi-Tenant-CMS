namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class UpdateTenantResult
{
    public int Id { get; set; }
    public string SubDomain { get; set; }
    public string CustomDomain { get; set; }
    public string Name { get; set; }
}