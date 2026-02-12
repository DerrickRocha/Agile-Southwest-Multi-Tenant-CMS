using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(): ControllerBase
{
    [HttpPost("register")]
    public Task<IActionResult> Register()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
    
}