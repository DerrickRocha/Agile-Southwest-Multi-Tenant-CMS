using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController(): ControllerBase
{
    [HttpPost("login")]
    public Task<IActionResult> Login()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
    
}