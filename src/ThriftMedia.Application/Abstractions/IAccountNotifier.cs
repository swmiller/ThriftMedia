using ThriftMedia.Domain.Events;

namespace ThriftMedia.Application.Abstractions;

public interface IAccountNotifier
{
    /// <summary>
    /// Sends the notification emails implied by the given domain events (approval, rejection,
    /// suspension, reactivation). Call only after the change has been committed. Delivery
    /// failures are logged and never thrown.
    /// </summary>
    Task NotifyAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken);
}
