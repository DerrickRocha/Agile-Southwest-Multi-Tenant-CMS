using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

public class CmsUser
{
    public Guid CmsUserId { get; set; }
    
    public string CognitoUserId { get; set; }
    
    public string Email { get; set; }
    
    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}