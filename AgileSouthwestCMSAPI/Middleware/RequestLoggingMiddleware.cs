using System.Diagnostics;

namespace AgileSouthwestCMSAPI.Middleware;

public sealed class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();

            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response?.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }

}
