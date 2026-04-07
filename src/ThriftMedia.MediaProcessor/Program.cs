using Azure.Messaging.ServiceBus;
using ThriftMedia.MediaProcessor;
using ThriftMedia.Mediator;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults (telemetry, health checks, etc.)
builder.AddServiceDefaults();

// Add Mediator
builder.Services.AddMediator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ThriftMedia.Application.Commands.ProcessMediaCommand).Assembly);
});

// Add hosted service
builder.Services.AddHostedService<MediaProcessorWorker>();

var host = builder.Build();
host.Run();
