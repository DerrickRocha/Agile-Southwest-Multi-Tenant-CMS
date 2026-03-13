namespace AgileSouthwestCMSAPI.Api.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipTenantResolutionAttribute: Attribute;