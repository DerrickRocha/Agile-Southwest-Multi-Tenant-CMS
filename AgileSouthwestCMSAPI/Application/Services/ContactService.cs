using AgileSouthwestCMSAPI.Api.Requests.Contact;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Mail;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ContactService(IEmailSender sender): IContactService
{
    public Task SubmitContactForm(SubmitContactFormRequest request)
    {
        var email = new EmailMessage(To:"smilingmoonfarmtech@gmail.com", Subject:"Contact Form Submission", HtmlBody:request.Message, ReplyTo:request.Email);
        sender.SendAsync(email);
        throw new NotImplementedException();
    }
}