using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Middleware;

public static class ApplicationBuilderExtensions
{
    public static void UseApiExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var env = context.RequestServices
                    .GetRequiredService<IHostEnvironment>();

                var feature = context.Features
                    .Get<IExceptionHandlerFeature>();

                var exception = feature?.Error;
                
                var statusCode = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };

                var problem = new ProblemDetails
                {
                    Title = "An unexpected error occurred",
                    Status = statusCode,
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
                
                problem.Type = "https://httpstatuses.com/500";

                problem.Extensions["traceId"] =
                    context.TraceIdentifier;

                context.Response.StatusCode =
                    problem.Status.Value;

                context.Response.ContentType =
                    "application/problem+json";

                await context.Response.WriteAsJsonAsync(problem);
            });
        });
    }
}