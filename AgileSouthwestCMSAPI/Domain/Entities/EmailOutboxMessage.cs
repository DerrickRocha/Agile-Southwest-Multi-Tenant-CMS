using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class EmailOutboxMessage
{
    public int Id { get; set; }
    public int TenantId { get; set; }           // plain column: the worker has no tenant context
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
 
    public EmailType Type { get; set; }
    public EmailOutboxStatus Status { get; set; } = EmailOutboxStatus.Pending;
    public int Attempts { get; set; }
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string? LastError { get; set; }
 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}