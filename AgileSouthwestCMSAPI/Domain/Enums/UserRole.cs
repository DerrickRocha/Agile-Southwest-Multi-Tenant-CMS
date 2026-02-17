namespace AgileSouthwestCMSAPI.Domain.Enums;

public enum UserRole
{
    Owner = 0,        // Created the tenant, full control
    Admin = 1,        // Can manage users + settings
    Editor = 2,       // Can create/edit content
    Member = 3        // Basic access
}