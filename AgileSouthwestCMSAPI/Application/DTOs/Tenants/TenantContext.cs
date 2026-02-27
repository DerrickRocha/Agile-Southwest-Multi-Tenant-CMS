using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class TenantContext : ITenantContext
{
    public CmsUser? User { get; private set; }
    public Tenant? Tenant { get; private set; }
    public UserTenant? Membership { get; private set; }

    public bool IsResolved { get; private set; }

    public void Set(CmsUser user, Tenant tenant, UserTenant membership)
    {
        User = user;
        Tenant = tenant;
        Membership = membership;
        IsResolved = true;
    }
}