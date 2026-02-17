using AgileSouthwestCMSAPI.Domain.DTOs;
using AgileSouthwestCMSAPI.Infrastructure.Exceptions;
using AgileSouthwestCMSAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Application.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(
    IAuthService service,
    ICognitoService cognito
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
        catch (UserNotConfirmedAuthException ex)
        {
            return StatusCode(403, new { code = "USER_NOT_CONFIRMED", message = ex.Message });
        }
        catch (CognitoValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmSignupRequest request)
    {
        try
        {
            await cognito.ConfirmSignUpAsync(request.Email, request.ConfirmationCode);
            return Ok(new { message = "Confirmed." });
        }
        catch (CognitoValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
    {
        try
        {
            await cognito.ResendConfirmationCodeAsync(request.Email);
            return Ok(new { message = "Confirmation code resent." });
        }
        catch (CognitoValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}