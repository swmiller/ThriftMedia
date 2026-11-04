using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Abstractions;

namespace ThriftMedia.Infrastructure.Email;

/// <summary>
/// Placeholder <see cref="IEmailSender"/> that only logs. Replace with a real provider before go-live.
/// The body is deliberately not logged (it may contain personal data).
/// </summary>
internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email (not sent, logging sender) to {Recipient}: {Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }
}
