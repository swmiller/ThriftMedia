using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Events;

namespace ThriftMedia.Application.Common;

internal sealed class EmailAccountNotifier(IEmailSender emailSender, ILogger<EmailAccountNotifier> logger) : IAccountNotifier
{
    public async Task NotifyAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in events)
        {
            var message = ToMessage(domainEvent);
            if (message is null) continue;

            try
            {
                await emailSender.SendAsync(message, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The state change is already committed; a failed email must not undo or fail it.
                logger.LogError(ex, "Failed to send {EventType} notification email", domainEvent.GetType().Name);
            }
        }
    }

    private static EmailMessage? ToMessage(DomainEvent domainEvent) => domainEvent switch
    {
        StoreApplicationApprovedEvent e => new EmailMessage(
            e.ApplicantEmail,
            "Your ThriftMedia store application was approved",
            $"Good news: the application for \"{e.StoreName}\" was approved. " +
            $"Sign in to the ThriftMedia admin site with a Google or Microsoft account that uses this email address ({e.ApplicantEmail})."),

        StoreApplicationRejectedEvent e => new EmailMessage(
            e.ApplicantEmail,
            "Your ThriftMedia store application was not approved",
            $"We were unable to approve the application for \"{e.StoreName}\"." +
            (string.IsNullOrWhiteSpace(e.Reason) ? string.Empty : $" Reason: {e.Reason}")),

        AppUserSuspendedEvent e => new EmailMessage(
            e.Email,
            "Your ThriftMedia account was suspended",
            "Your ThriftMedia store account has been suspended and you can no longer sign in. Contact ThriftMedia support for details."),

        AppUserReactivatedEvent e => new EmailMessage(
            e.Email,
            "Your ThriftMedia account was reactivated",
            "Your ThriftMedia store account has been reactivated. You can sign in again."),

        _ => null
    };
}
