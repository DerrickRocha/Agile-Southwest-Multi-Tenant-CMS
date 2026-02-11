using System.Security.Claims;

namespace AgileSouthwestCMSAPI.Domain.ValueObjects;

public interface ITenantContext
{
    string TenantId { get; set; }
    string UserId { get; set; }
}

class TenantContext : ITenantContext
{
    public string TenantId { get; set; }
    public string UserId { get; set; }

    public TenantContext(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        TenantId = user?.FindFirst("custom:tenant_id")?.Value ?? throw new UnauthorizedAccessException("Tenant not found");
        UserId = (user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value) ?? throw new UnauthorizedAccessException("User not found");
    }
}