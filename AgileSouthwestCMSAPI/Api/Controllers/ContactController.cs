using AgileSouthwestCMSAPI.Api.Middleware;
using AgileSouthwestCMSAPI.Api.Requests.Contact;
using AgileSouthwestCMSAPI.Domain.Mail;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[SkipTenantResolution]
public class ContactController(IEmailSender emailSender): ControllerBase
{
    private IEmailSender EmailSender { get; } = emailSender;

    [HttpPost]
    public async Task<IActionResult> SubmitContactForm(SubmitContactFormRequest request)
    {
        var message = new EmailMessage("derrickrocha4@gmail.com", "Contact Form Submission", request.Message);
        await EmailSender.SendAsync(message);
        return Ok();
    }
    
}