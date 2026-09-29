
namespace AgileSouthwestCMSAPI.Domain.Mail;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}