using System.Net;
using AgileSouthwestCMSAPI.Api.Requests.Contact;
using AgileSouthwestCMSAPI.Application.Exceptions;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Mail;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ContactService(IEmailSender sender) : IContactService
{
    public async Task SubmitContactForm(SubmitContactFormRequest request)
    {
        if (string.IsNullOrEmpty(request.Message))
        {
            throw new ContactServiceError("Message must not be empty.");
        }

        if (string.IsNullOrEmpty(request.Subject))
        {
            throw new ContactServiceError("Subject must not be empty.");
        }

        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ContactServiceError("Email must not be empty.");       
        }

        if (string.IsNullOrEmpty(request.FirstName))
        {
            throw new ContactServiceError("First name must not be empty.");       
        }

        if (string.IsNullOrEmpty(request.LastName))
        {
            throw new ContactServiceError("Last name must not be empty.");      
        }

        try
        {
            var email = new EmailMessage(
                To: "smilingmoonfarmtech@gmail.com",
                Subject: request.Subject,
                HtmlBody: BuildHtmlBody(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Message
                ),
                ReplyTo: request.Email);
            await sender.SendAsync(email);
        }
        catch (Exception e)
        {
            throw new ContactServiceError("Could not send email. Please try again later.");
        }
        
    }

    private string BuildHtmlBody(string firstName, string lastName, string email, string message)
    {
        // Encode everything that came from the visitor so it can't inject HTML.
        var name = WebUtility.HtmlEncode($"{firstName} {lastName}".Trim());
        var safeEmail = WebUtility.HtmlEncode(email?.Trim() ?? string.Empty);

        // Encode first, then convert line breaks so the message keeps its formatting.
        var body = WebUtility.HtmlEncode(message)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", "<br />");

        return $"""
                <!DOCTYPE html>
                <html lang="en">
                <body style="margin:0;padding:24px;background-color:#f4f5f7;font-family:Arial,Helvetica,sans-serif;color:#222222;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                    <tr>
                      <td align="center">
                        <table role="presentation" width="600" cellpadding="0" cellspacing="0"
                               style="max-width:600px;background-color:#ffffff;border-radius:6px;padding:24px;">
                          <tr>
                            <td>
                              <h2 style="margin:0 0 16px 0;font-size:20px;">New contact form message</h2>
                              <p style="margin:0 0 8px 0;"><strong>From:</strong> {name}</p>
                              <p style="margin:0 0 16px 0;"><strong>Email:</strong>
                                <a href="mailto:{safeEmail}" style="color:#1a5fb4;">{safeEmail}</a>
                              </p>
                              <div style="padding:16px;background-color:#f9fafb;border-left:4px solid #cccccc;line-height:1.5;">
                                {body}
                              </div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
    }
}