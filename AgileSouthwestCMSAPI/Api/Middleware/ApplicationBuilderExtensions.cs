using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Middleware;

public static class ApplicationBuilderExtensions
{
    public static void UseApiExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }
                
                var env = context.RequestServices
                    .GetRequiredService<IHostEnvironment>();
                
                var exception = context.Features
                    .Get<IExceptionHandlerFeature>()?
                    .Error;
                
                var statusCode = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };
                
                var title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Bad request",
                    StatusCodes.Status401Unauthorized => "Unauthorized",
                    StatusCodes.Status404NotFound => "Resource not found",
                    _ => "An unexpected error occurred"
                };

                var problem = new ProblemDetails
                {
                    Title = title,
                    Status = statusCode,
                    Type = $"https://httpstatuses.com/{statusCode}",
                    Instance = context.Request.Path
                };

                if (env.IsDevelopment() && exception != null)
                {
                    problem.Detail = exception.Message;

                    problem.Extensions["exception"] = new
                    {
                        exception.GetType().Name,
                        exception.StackTrace
                    };
                }
                
                problem.Extensions["traceId"] =
                    context.TraceIdentifier;

                context.Response.StatusCode = statusCode;

                context.Response.ContentType =
                    "application/problem+json";

                await context.Response.WriteAsJsonAsync(problem);
            });
        });
    }
}