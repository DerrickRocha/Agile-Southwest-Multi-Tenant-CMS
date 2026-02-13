using AgileSouthwestCMSAPI.Domain.DTOs;
using AgileSouthwestCMSAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Controllers;

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
        var result = await service.SignupAsync(request);
        return StatusCode(result.StatusCode, result);
    }
}