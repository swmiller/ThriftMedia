using Microsoft.Extensions.DependencyInjection;

namespace ThriftMedia.Mediator;

/// <summary>
/// Default mediator implementation that coordinates requests and notifications through handlers.
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");
        }

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));
        
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handle method not found on handler for {requestType.Name}");
        }

        var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });

        if (result is Task<TResponse> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler for {requestType.Name} did not return expected Task<{typeof(TResponse).Name}>");
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        if (notification == null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        var handlers = _serviceProvider.GetServices(handlerType);

        if (handlers == null || !handlers.Any())
        {
            // No handlers registered - this is acceptable for notifications
            return;
        }

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<INotification>.Handle));

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Handle method not found on notification handler for {notificationType.Name}");
        }

        var tasks = handlers.Select(handler =>
        {
            var result = handleMethod.Invoke(handler, new object[] { notification, cancellationToken });
            return result as Task ?? Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }
}
