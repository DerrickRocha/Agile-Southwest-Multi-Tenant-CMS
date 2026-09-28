using AgileSouthwestCMSAPI.Api.Requests.Contact;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ContactController: ControllerBase
{
    [HttpPost]
    public IActionResult SubmitContactForm(SubmitContactFormRequest request)
    {
        return Ok();
    }
    
}