using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface ITenantContext
{
    CmsUser User { get; }
    Tenant Tenant { get; }
    UserTenant Membership { get; }
    bool IsResolved { get; }
}