using Azure.Messaging.ServiceBus;
using ThriftMedia.MediaProcessor;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults (telemetry, health checks, etc.)
builder.AddServiceDefaults();

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ThriftMedia.Application.Commands.ProcessMediaCommand).Assembly);
});

// Add Azure Service Bus
var serviceBusConnectionString = builder.Configuration.GetConnectionString("ServiceBus")
    ?? throw new InvalidOperationException("ServiceBus connection string not configured");

builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));

// Register application services and pipeline
// TODO: Register pipeline steps, repositories, and services from Infrastructure layer
// builder.Services.AddTransient<IMediaProcessingStep, OcrProcessingStep>();
// builder.Services.AddTransient<IMediaProcessingStep, MediaClassificationStep>();
// builder.Services.AddTransient<IMediaProcessingStep, ContentModerationStep>();
// builder.Services.AddTransient<IMediaProcessingStep, CatalogListingStep>();
// builder.Services.AddTransient<MediaProcessingPipeline>();

// Add hosted service
builder.Services.AddHostedService<MediaProcessorWorker>();

var host = builder.Build();
host.Run();
