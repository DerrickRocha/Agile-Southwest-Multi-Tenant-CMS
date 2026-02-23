using System.ComponentModel.DataAnnotations.Schema;
using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Domain.Entities;

[Table("cms_users")]
public class CmsUser
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("cognito_user_id")]
    public string CognitoUserId { get; set; }
    
    [Column("email")]
    public string Email { get; set; }
    
    [Column("role")]
    public UserRole Role { get; set; }

    [Column("status")]
    public UserStatus Status { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}