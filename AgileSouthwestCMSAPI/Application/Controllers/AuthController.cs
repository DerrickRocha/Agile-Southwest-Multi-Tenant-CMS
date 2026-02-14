using AgileSouthwestCMSAPI.Domain.DTOs;
using AgileSouthwestCMSAPI.Infrastructure.Exceptions;
using AgileSouthwestCMSAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Application.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(
    IAuthService service
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SignupRequest request)
    {
        try
        {
            var result = await service.SignupAsync(request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (CognitoValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var tokens = await service.AuthenticateAsync(request.Email, request.Password);
            return Ok(tokens);
        }
        catch (CognitoValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}