using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public Guid TenantId { get; set; } 
    
    public string CognitoSub { get; set; }
    
    public string Email { get; set; }
    
    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public Tenant Tenant { get; set; }
}