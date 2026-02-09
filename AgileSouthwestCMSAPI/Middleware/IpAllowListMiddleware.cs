using System.Net;

namespace AgileSouthwestCMSAPI.Middleware;

public sealed class IpAllowListMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<IPAddress> _allowedIps;

    public IpAllowListMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<IpAllowListMiddleware> logger)
    {
        _next = next;
        _allowedIps = [];

        var ips = configuration
            .GetSection("HealthChecks:AllowedIPs")
            .Get<string[]>() ?? [];

        foreach (var ip in ips)
        {
            if (IPAddress.TryParse(ip, out var parsed))
            {
                if (parsed.IsIPv4MappedToIPv6)
                {
                    parsed = parsed.MapToIPv4();
                }

                _allowedIps.Add(parsed);
            }
            else
            {
                logger.LogWarning("Invalid IP address in HealthChecks:AllowedIPs: {Ip}", ip);
            }
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;

        if (remoteIp is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        if (!_allowedIps.Contains(remoteIp))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }
}

