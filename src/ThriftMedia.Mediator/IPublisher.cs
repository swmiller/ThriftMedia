namespace ThriftMedia.Mediator;

/// <summary>
/// Defines a publisher for notifications.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publish a notification to multiple handlers.
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation</returns>
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}
