using System.Net;
using AgileSouthwestCMSAPI.Api.Requests.Contact;
using AgileSouthwestCMSAPI.Application.Exceptions;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Mail;
using AgileSouthwestCMSAPI.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ContactService(
    IEmailSender sender,
    IOptions<ContactOptions> options,
    ILogger<ContactService> logger
) : IContactService
{
    private const int MaxNameLength = 100;
    private const int MaxSubjectLength = 150;
    private const int MaxMessageLength = 5000;

    public async Task SubmitContactForm(SubmitContactFormRequest request, CancellationToken ct = default)
    {
        var firstName = Require(request.FirstName, "First name", MaxNameLength);
        var lastName = Require(request.LastName, "Last name", MaxNameLength);
        var subject = Require(request.Subject, "Subject", MaxSubjectLength);
        var message = Require(request.Message, "Message", MaxMessageLength);
        var email = ParseEmail(request.Email);

        if (subject.Contains('\r') || subject.Contains('\n'))
            throw new ContactValidationException("Subject must be a single line.");

        try
        {
            var emailMessage = new EmailMessage(
                To: options.Value.Recipient,
                Subject: $"[Contact] {subject}",
                HtmlBody: BuildHtmlBody(firstName, lastName, email, message),
                TextBody: $"New contact form message\nFrom: {firstName} {lastName}\nEmail: {email}\n\n{message}",
                ReplyTo: email);

            await sender.SendAsync(emailMessage, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contact form email");
            throw new ContactServiceError("Could not send email. Please try again later.");
        }
    }

    private static string ParseEmail(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ContactValidationException("Email must not be empty.");

        if (trimmed.Length > 254 ||
            !MailboxAddress.TryParse(trimmed, out var mailbox) ||
            !mailbox.Address.Contains('@'))
            throw new ContactValidationException("Email address is not valid.");

        return mailbox.Address; // address only, drops any "Display Name <...>" part
    }

    private static string Require(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ContactValidationException($"{field} must not be empty.");
        if (trimmed.Length > maxLength)
            throw new ContactValidationException($"{field} must be at most {maxLength} characters.");
        return trimmed;
    }

    private string BuildHtmlBody(string firstName, string lastName, string email, string message)
    {
        // Encode everything that came from the visitor so it can't inject HTML.
        var name = WebUtility.HtmlEncode($"{firstName} {lastName}".Trim());
        var safeEmail = WebUtility.HtmlEncode(email);
        var href = WebUtility.HtmlEncode(Uri.EscapeDataString(email));

        var body = WebUtility.HtmlEncode(message)
            .Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br />");


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
                                <a href="mailto:{href}" style="color:#1a5fb4;">{safeEmail}</a>
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

public class ContactValidationException(string message) : ContactServiceError(message);