using System.Text.Json;
using AgileSouthwestCMSAPI.Application.Exceptions;

namespace AgileSouthwestCMSAPI.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UserNotConfirmedAuthException ex)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                code = "USER_NOT_CONFIRMED",
                message = ex.Message
            });

            await context.Response.WriteAsync(payload);
        }
        catch (CognitoValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                code = "COGNITO_VALIDATION_ERROR",
                message = ex.Message
            });

            await context.Response.WriteAsync(payload);
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                code = "CONFLICT",
                message = ex.Message
            });

            await context.Response.WriteAsync(payload);
        }
        catch
        {
            // Avoid leaking details; log internally if you have logging wired up.
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                code = "INTERNAL_SERVER_ERROR",
                message = "An unexpected error occurred."
            });

            await context.Response.WriteAsync(payload);
        }
    }
}