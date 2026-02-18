using AgileSouthwestCMSAPI.Application.DTOs.Auth;
using AgileSouthwestCMSAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;
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
        var result = await service.SignupAsync(request);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tokens = await service.AuthenticateAsync(request.Email, request.Password);
        return Ok(tokens);
    }
    
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmSignupRequest request)
    {
        await cognito.ConfirmSignUpAsync(request.Email, request.ConfirmationCode);
        return Ok(new { message = "Confirmed." });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
    {
        await cognito.ResendConfirmationCodeAsync(request.Email);
        return Ok(new { message = "Confirmation code resent." });
    }
}