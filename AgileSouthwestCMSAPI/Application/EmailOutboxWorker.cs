using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Domain.Mail;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application;

public class EmailOutboxWorker(IServiceScopeFactory scopeFactory, ILogger<EmailOutboxWorker> logger)
    : BackgroundService
{
    private const int MaxAttempts = 5;
    private const int BatchSize = 20;
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessBatchAsync(stoppingToken);
                if (processed == 0) await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email outbox batch failed");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
 
    private async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        // Background services are singletons, so every batch gets its own scope.
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
        var composer = scope.ServiceProvider.GetRequiredService<IOrderEmailComposer>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>(); // later: IEmailSenderFactory per tenant
 
        var now = DateTime.UtcNow;
 
        // Assumes ONE app instance. With several, claim rows first (e.g. SELECT ... FOR UPDATE SKIP LOCKED
        // inside a transaction) or two workers can send the same email.
        var batch = await db.EmailOutbox
            .Where(m => m.Status == EmailOutboxStatus.Pending && m.NextAttemptAt <= now)
            .OrderBy(m => m.NextAttemptAt)
            .Take(BatchSize)
            .ToListAsync(ct);
 
        foreach (var message in batch)
        {
            try
            {
                var email = await composer.ComposeAsync(message, ct);
                await sender.SendAsync(email, ct);
 
                message.Status = EmailOutboxStatus.Sent;
                message.SentAt = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.Attempts++;
                message.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
 
                if (message.Attempts >= MaxAttempts)
                {
                    message.Status = EmailOutboxStatus.Failed;
                    logger.LogError(ex, "Giving up on email {EmailId} (type {Type}, order {OrderId})",
                        message.Id, message.Type, message.OrderId);
                }
                else
                {
                    // 1, 2, 4, 8 minutes
                    message.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, message.Attempts - 1));
                    logger.LogWarning(ex, "Email {EmailId} failed (attempt {Attempt}), will retry",
                        message.Id, message.Attempts);
                }
            }
 
            // Save after each message so a crash can't make us resend ones already sent.
            await db.SaveChangesAsync(ct);
        }
 
        return batch.Count;
    }
}
