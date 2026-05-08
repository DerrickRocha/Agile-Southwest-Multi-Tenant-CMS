namespace AgileSouthwestCMSAPI.Domain.Entities;

public class Settings
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public double TaxRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}