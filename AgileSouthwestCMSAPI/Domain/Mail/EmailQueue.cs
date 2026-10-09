using System.Threading.Channels;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Domain.Mail;


public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel =
        Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(message, ct);

    public IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}