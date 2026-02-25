# ThriftMedia.Mediator

A custom implementation of the Mediator pattern that provides MediatR-compatible interfaces and functionality without external dependencies.

## Why This Exists

This library was created to replace MediatR after it transitioned to a paid licensing model. It provides API-compatible interfaces and core functionality for the mediator pattern, enabling seamless migration from MediatR.

## Features

- **Request/Response Pattern**: Send commands and queries with typed responses
- **Notification Pattern**: Publish events to multiple handlers
- **Dependency Injection**: Automatic handler registration via assembly scanning
- **API Compatible**: Drop-in replacement for MediatR with identical interfaces

## Core Interfaces

### IRequest<TResponse>
Marker interface for requests that return a response.

```csharp
public record GetMediaQuery(Guid MediaId) : IRequest<MediaDto>;
```

### IRequestHandler<TRequest, TResponse>
Handler for processing requests.

```csharp
public class GetMediaQueryHandler : IRequestHandler<GetMediaQuery, MediaDto>
{
    public async Task<MediaDto> Handle(GetMediaQuery request, CancellationToken cancellationToken)
    {
        // Handle the request
    }
}
```

### INotification
Marker interface for notifications (events).

```csharp
public record MediaProcessedNotification(Guid MediaId) : INotification;
```

### INotificationHandler<TNotification>
Handler for processing notifications. Multiple handlers can subscribe to the same notification.

```csharp
public class MediaProcessedNotificationHandler : INotificationHandler<MediaProcessedNotification>
{
    public async Task Handle(MediaProcessedNotification notification, CancellationToken cancellationToken)
    {
        // Handle the notification
    }
}
```

### IMediator
The main mediator interface that combines sending requests and publishing notifications.

```csharp
public class MyService
{
    private readonly IMediator _mediator;

    public MyService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DoWork()
    {
        // Send a request
        var result = await _mediator.Send(new GetMediaQuery(mediaId));

        // Publish a notification
        await _mediator.Publish(new MediaProcessedNotification(mediaId));
    }
}
```

## Registration

Register the mediator and all handlers from an assembly:

```csharp
services.AddMediator(config =>
{
    config.RegisterServicesFromAssembly(typeof(ProcessMediaCommand).Assembly);
});
```

Or from multiple assemblies:

```csharp
services.AddMediator(config =>
{
    config.RegisterServicesFromAssemblies(
        typeof(ApplicationCommand).Assembly,
        typeof(DomainEvent).Assembly
    );
});
```

Or using a type marker:

```csharp
services.AddMediator(config =>
{
    config.RegisterServicesFromAssemblyContaining<ProcessMediaCommand>();
});
```

## Migration from MediatR

To migrate from MediatR to ThriftMedia.Mediator:

1. Remove the MediatR package reference from your `.csproj` files
2. Add a reference to ThriftMedia.Mediator
3. Update using statements from `using MediatR;` to `using ThriftMedia.Mediator;`
4. Update service registration from `AddMediatR` to `AddMediator`

### Before:
```csharp
using MediatR;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessMediaCommand).Assembly);
});
```

### After:
```csharp
using ThriftMedia.Mediator;

builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessMediaCommand).Assembly);
});
```

All existing request handlers, notification handlers, and mediator usage will continue to work without modification.

## Limitations

This implementation provides the core mediator functionality used by most applications. Advanced MediatR features not currently implemented include:

- Pipeline behaviors (pre/post-processing)
- Stream requests
- Request pre-processors and post-processors

If you need these features, they can be added to this library as needed.

## License

This library is part of the ThriftMedia project and is licensed under the MIT License.
