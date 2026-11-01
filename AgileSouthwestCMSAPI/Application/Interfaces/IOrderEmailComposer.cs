using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Mail;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IOrderEmailComposer
{
    // Loads tenant branding and sender settings, picks the template for message.Type, HTML-encodes
    // every customer-supplied value, and returns an EmailMessage (with both HTML and plain-text bodies).
    Task<EmailMessage> ComposeAsync(EmailOutboxMessage message, CancellationToken ct);
}
