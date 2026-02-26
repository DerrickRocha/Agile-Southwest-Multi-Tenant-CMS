using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Application.DTOs.Tenants;

public class TenantContext : ITenantContext
{
    public CmsUser User { get; internal set; } = null!;
    public Tenant Tenant { get; internal set; } = null!;
    public UserTenant Membership { get; internal set; } = null!;
    public bool IsResolved { get; internal set; }
}