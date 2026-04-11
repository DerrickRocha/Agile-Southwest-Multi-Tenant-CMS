namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Stores
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string SubDomain { get; set; }
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}