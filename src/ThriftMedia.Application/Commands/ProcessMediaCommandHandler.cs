using ThriftMedia.Mediator;
using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Pipelines;
using ThriftMedia.Application.Repositories;

namespace ThriftMedia.Application.Commands;

/// <summary>
/// Handler for processing media through the ingestion pipeline.
/// </summary>
public class ProcessMediaCommandHandler : IRequestHandler<ProcessMediaCommand, bool>
{
    private readonly MediaProcessingPipeline _pipeline;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<ProcessMediaCommandHandler> _logger;

    public ProcessMediaCommandHandler(
        MediaProcessingPipeline pipeline,
        IMediaRepository mediaRepository,
        ILogger<ProcessMediaCommandHandler> logger)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(ProcessMediaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing media command for MediaId: {MediaId}, StoreId: {StoreId}",
            request.MediaId,
            request.StoreId);

        try
        {
            // Update media status to Processing
            var media = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);
            if (media == null)
            {
                _logger.LogError("Media not found: {MediaId}", request.MediaId);
                return false;
            }

            media.StartProcessing("system", DateTime.UtcNow);
            await _mediaRepository.UpdateAsync(media, cancellationToken);

            // Create processing context
            var context = new MediaProcessingContext(
                request.MediaId,
                request.StoreId,
                request.ImageUri);

            // Execute pipeline
            var result = await _pipeline.ExecuteAsync(context, cancellationToken);

            if (!result.Success)
            {
                _logger.LogError(
                    "Media processing failed for MediaId: {MediaId}. Error: {ErrorMessage}",
                    request.MediaId,
                    result.ErrorMessage);

                // Mark media as failed
                media.MarkAsFailed("system", DateTime.UtcNow);
                await _mediaRepository.UpdateAsync(media, cancellationToken);

                return false;
            }

            _logger.LogInformation(
                "Media processing completed successfully for MediaId: {MediaId}",
                request.MediaId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error processing media for MediaId: {MediaId}",
                request.MediaId);

            return false;
        }
    }
}
