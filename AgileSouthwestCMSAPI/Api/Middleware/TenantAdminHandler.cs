using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AgileSouthwestCMSAPI.Api.Middleware;

public class TenantAdminHandler(ITenantContext tenantContext) : AuthorizationHandler<TenantAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAdminRequirement requirement)
    {
        if (tenantContext.Membership?.Role == UserTenantRole.Admin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}