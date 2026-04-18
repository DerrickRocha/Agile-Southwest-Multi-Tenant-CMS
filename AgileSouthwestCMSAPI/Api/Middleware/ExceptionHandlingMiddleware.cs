using System.ComponentModel.DataAnnotations;
using AgileSouthwestCMSAPI.Application.Exceptions;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Mvc;

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
            await WriteProblem(context, StatusCodes.Status401Unauthorized, "User Not Confirmed", ex.Message);
        }
        catch (CognitoValidationException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Cognito Validation Error", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblem(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (NotFoundException exception)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, "Not Found", exception.Message);
        }
        catch (ValidationException exception)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation error", exception.Message);
        }
        catch
        {
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.");
        }
    }
    
    private static Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status
        });
    }
}