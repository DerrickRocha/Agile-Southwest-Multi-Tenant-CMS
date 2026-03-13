namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class UpdateTenantRequest
{
    public int Id { get; set; }
    public string Name {set; get;}
    public string SubDomain {set; get;}
    public string? CustomDomain {set; get;}
    
    public byte[] RowVersion { get; set; }
}