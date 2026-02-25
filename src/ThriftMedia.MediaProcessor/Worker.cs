using Azure.Messaging.ServiceBus;
using ThriftMedia.Mediator;
using System.Text.Json;
using ThriftMedia.Application.Commands;

namespace ThriftMedia.MediaProcessor;

/// <summary>
/// Background worker service that listens to Azure Service Bus queue for media processing messages.
/// </summary>
public class MediaProcessorWorker : BackgroundService
{
    private readonly ILogger<MediaProcessorWorker> _logger;
    private readonly IMediator _mediator;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName;
    private ServiceBusProcessor? _processor;

    public MediaProcessorWorker(
        ILogger<MediaProcessorWorker> logger,
        IMediator mediator,
        ServiceBusClient serviceBusClient,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        _queueName = configuration["ServiceBus:QueueName"] ?? "media-processing";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Media Processor Worker starting...");

        // Create processor
        _processor = _serviceBusClient.CreateProcessor(_queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        // Register message handler
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        // Start processing
        await _processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation("Media Processor Worker started and listening to queue: {QueueName}", _queueName);

        // Keep the service running
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received message: {Body}", body);

            // Deserialize message to ProcessMediaCommand
            var command = JsonSerializer.Deserialize<ProcessMediaCommand>(body);
            if (command == null)
            {
                _logger.LogError("Failed to deserialize message");
                await args.DeadLetterMessageAsync(args.Message, "Deserialization failed");
                return;
            }

            // Process the command through MediatR
            var success = await _mediator.Send(command, args.CancellationToken);

            if (success)
            {
                // Complete the message
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("Message processed successfully for MediaId: {MediaId}", command.MediaId);
            }
            else
            {
                // Move to dead letter queue
                await args.DeadLetterMessageAsync(args.Message, "Processing failed");
                _logger.LogError("Message processing failed for MediaId: {MediaId}", command.MediaId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            await args.DeadLetterMessageAsync(args.Message, ex.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Error from Service Bus: {ErrorSource}, Entity: {EntityPath}",
            args.ErrorSource,
            args.EntityPath);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Media Processor Worker stopping...");

        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
