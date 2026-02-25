# MediatR Migration Guide

This document outlines the steps to migrate from MediatR to the custom ThriftMedia.Mediator implementation.

## Why Migrate?

MediatR has transitioned to a paid licensing model. ThriftMedia.Mediator is a custom implementation that provides the same core functionality with an API-compatible interface, allowing for seamless migration.

## What Was Created

A new project `ThriftMedia.Mediator` has been created with the following components:

### Core Interfaces (MediatR-compatible)
- `IRequest<TResponse>` - Marker for requests with responses
- `IRequestHandler<TRequest, TResponse>` - Handler for requests
- `INotification` - Marker for notification messages
- `INotificationHandler<TNotification>` - Handler for notifications
- `IMediator` - Main mediator interface
- `ISender` - Interface for sending requests
- `IPublisher` - Interface for publishing notifications
- `Unit` - Type representing void responses

### Implementation
- `Mediator` - Main mediator implementation using reflection and DI
- `ServiceCollectionExtensions` - Extension methods for registering services
- `MediatorConfiguration` - Configuration builder for assembly scanning

## Migration Steps

### 1. Add Project Reference to ThriftMedia.Mediator

Update the following project files to reference ThriftMedia.Mediator instead of the MediatR package:

#### ThriftMedia.Application.csproj
**Remove:**
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
```

**Add:**
```xml
<ProjectReference Include="..\ThriftMedia.Mediator\ThriftMedia.Mediator.csproj" />
```

#### ThriftMedia.Infrastructure.csproj
**Remove:**
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
```

**Add:**
```xml
<ProjectReference Include="..\ThriftMedia.Mediator\ThriftMedia.Mediator.csproj" />
```

#### ThriftMedia.MediaProcessor.csproj
**Remove:**
```xml
<PackageReference Include="MediatR" Version="12.4.1" />
```

**Add:**
```xml
<ProjectReference Include="..\ThriftMedia.Mediator\ThriftMedia.Mediator.csproj" />
```

### 2. Update Using Statements

In the following files, change the using statement:

**From:**
```csharp
using MediatR;
```

**To:**
```csharp
using ThriftMedia.Mediator;
```

**Files to update:**
- `src/ThriftMedia.Application/Commands/ProcessMediaCommand.cs`
- `src/ThriftMedia.Application/Commands/ProcessMediaCommandHandler.cs`
- `src/ThriftMedia.MediaProcessor/Worker.cs`
- `src/ThriftMedia.MediaProcessor/Program.cs`

### 3. Update Service Registration

In `src/ThriftMedia.MediaProcessor/Program.cs`:

**From:**
```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ThriftMedia.Application.Commands.ProcessMediaCommand).Assembly);
});
```

**To:**
```csharp
builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ThriftMedia.Application.Commands.ProcessMediaCommand).Assembly);
});
```

### 4. Verify Build

After making these changes, rebuild the solution:

```powershell
dotnet build ThriftMedia.sln
```

### 5. Run Tests

Ensure all tests pass after migration:

```powershell
dotnet test ThriftMedia.sln
```

## Code Changes Summary

No changes are required to:
- Request/Command definitions (e.g., `ProcessMediaCommand`)
- Handler implementations (e.g., `ProcessMediaCommandHandler`)
- Mediator usage in services (e.g., `_mediator.Send(...)`)

The API is 100% compatible, so only package references, using statements, and registration method names need to change.

## Verification Checklist

- [ ] Remove MediatR package references from all `.csproj` files
- [ ] Add ThriftMedia.Mediator project references
- [ ] Update all `using MediatR;` to `using ThriftMedia.Mediator;`
- [ ] Update `AddMediatR` to `AddMediator` in service registration
- [ ] Build solution successfully
- [ ] Run all tests successfully
- [ ] Test media processing pipeline end-to-end

## Rollback Plan

If issues are encountered, you can rollback by:
1. Reverting the project reference changes
2. Restoring MediatR package references
3. Reverting using statement changes
4. Reverting service registration changes

Keep the ThriftMedia.Mediator project in the solution for future use.

## Future Enhancements

If additional MediatR features are needed (pipeline behaviors, stream requests, etc.), they can be added to ThriftMedia.Mediator. See the README in the ThriftMedia.Mediator project for current limitations.
