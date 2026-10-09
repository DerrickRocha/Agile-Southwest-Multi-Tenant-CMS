using AgileSouthwestCMSAPI.Domain.Mail;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailMessage message, CancellationToken ct = default);
    IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken ct);
}