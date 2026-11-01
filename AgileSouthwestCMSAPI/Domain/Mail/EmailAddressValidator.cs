using System.ComponentModel.DataAnnotations;
using MimeKit;

namespace AgileSouthwestCMSAPI.Domain.Mail;

public static class EmailAddressValidator
{
    public static string Normalize(string? value, string field = "Email")
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 254 ||
            !MailboxAddress.TryParse(trimmed, out var mailbox) || !mailbox.Address.Contains('@'))
            throw new ValidationException($"{field} is not a valid email address.");

        return mailbox.Address;   // address only: drops any "Display Name <...>" part
    }
}