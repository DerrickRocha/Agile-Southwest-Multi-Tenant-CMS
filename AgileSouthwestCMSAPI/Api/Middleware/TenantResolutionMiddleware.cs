using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;

namespace AgileSouthwestCMSAPI.Api.Middleware;

using Microsoft.EntityFrameworkCore;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        CmsDbContext db,
        TenantContext tenantContext)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Skip endpoints that do not require tenant context
        if (IsBypassPath(path))
        {
            await next(context);
            return;
        }

        // User must be authenticated at this point
        var sub = context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Tenant header required
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing X-Tenant-Id header");
            return;
        }

        if (!int.TryParse(tenantHeader, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid tenant id");
            return;
        }

        var membership = await db.UserTenants
            .Include(ut => ut.User)
            .Include(ut => ut.Tenant)
            .SingleOrDefaultAsync(ut =>
                ut.User.CognitoUserId == sub &&
                ut.TenantId == tenantId);

        if (membership == null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("User not in tenant");
            return;
        }

        tenantContext.User = membership.User;
        tenantContext.Tenant = membership.Tenant;
        tenantContext.Membership = membership;

        await next(context);
    }

    private static bool IsBypassPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.StartsWith("/auth") ||
               path.StartsWith("/health") ||
               path.StartsWith("/me/tenants");
    }
}