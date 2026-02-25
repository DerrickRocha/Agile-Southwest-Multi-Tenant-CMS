namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class AddTenantResult
{
    public int TenantId { get; set; }
    public string SubDomain { get; set; }
    public string CustomDomain { get; set; }
    public string Name { get; set; }
    public byte[] RowVersion { get; set; }

}