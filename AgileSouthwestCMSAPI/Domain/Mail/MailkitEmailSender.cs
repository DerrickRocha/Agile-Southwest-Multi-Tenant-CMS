using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgileSouthwestCMSAPI.Domain.Mail;

public class MailkitEmailSender(IOptions<EmailOptions> options): IEmailSender
{
    private readonly EmailOptions _options = options.Value;
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        msg.To.Add(MailboxAddress.Parse(message.To));
        if (message.ReplyTo is not null) msg.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
        msg.Subject = message.Subject;
        msg.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_options.User, _options.Password, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}