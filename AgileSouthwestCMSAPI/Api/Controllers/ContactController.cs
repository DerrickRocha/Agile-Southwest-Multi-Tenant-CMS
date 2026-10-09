using AgileSouthwestCMSAPI.Api.Middleware;
using AgileSouthwestCMSAPI.Api.Requests.Contact;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[SkipTenantResolution]
public class ContactController(IContactService service): ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitContactForm(SubmitContactFormRequest request)
    {
        await service.SubmitContactForm(request);
        return Ok();
    }
    
}