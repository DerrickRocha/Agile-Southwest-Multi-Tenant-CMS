using System.Security.Claims;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Amazon.CognitoIdentityProvider.Model;

namespace AgileSouthwestCMSAPI.Api.Middleware;

using Microsoft.EntityFrameworkCore;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        CmsDbContext db,
        ITenantContext tenantContext)
    {
        var path = context.Request.Path;

        // Skip endpoints that do not require tenant context
        if (IsBypassPath(path, context.Request.Method))
        {
            await next(context);
            return;
        }
        
        // User must be authenticated at this point
        var user = context.User ?? throw new UnauthorizedAccessException("No user principal found");

        var sub = (user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value);

        if (string.IsNullOrEmpty(sub))
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Tenant header required
        if (!context.Request.Headers.TryGetValue(TenantHeaders.TenantId, out var tenantHeader))
        {
            throw new BadHttpRequestException("Missing X-Tenant-Id header");
        }

        if (!int.TryParse(tenantHeader, out var tenantId))
        {
            throw new BadHttpRequestException("Invalid tenant id");
        }

        var membership = await db.UserTenants
            .Where(ut => ut.TenantId == tenantId && ut.User.CognitoUserId == sub)
            .Select(ut => new
            {
                ut.User,
                ut.Tenant,
                ut
            })
            .SingleOrDefaultAsync();

        if (membership == null)
        {
            throw new ForbiddenException("User is not a member of this tenant");
        }
        tenantContext.Set(membership.User, membership.Tenant, membership.ut);

        await next(context);
    }

    private static bool IsBypassPath(PathString path, string? method)
    {
        if (string.IsNullOrEmpty(method)) return false;

        return path.StartsWithSegments("/auth") ||
               path.StartsWithSegments("/health") ||
               path.StartsWithSegments("/me/tenants") ||
               (path.StartsWithSegments("/tenants") && method == "POST");
    }
}