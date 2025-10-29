using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Repositories;
using ThriftMedia.Application.Services;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Application.Pipelines.Steps;

/// <summary>
/// Processing step that classifies the media type based on OCR results.
/// </summary>
public class MediaClassificationStep : IMediaProcessingStep
{
    private readonly IMediaClassificationService _classificationService;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<MediaClassificationStep> _logger;

    public string StepName => "Media Classification";

    public MediaClassificationStep(
        IMediaClassificationService classificationService,
        IMediaRepository mediaRepository,
        ILogger<MediaClassificationStep> logger)
    {
        _classificationService = classificationService ?? throw new ArgumentNullException(nameof(classificationService));
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting media classification for MediaId: {MediaId}", context.MediaId);

            // Load media entity
            var media = await _mediaRepository.GetByIdAsync(context.MediaId, cancellationToken);
            if (media == null)
            {
                return MediaProcessingResult.Failed($"Media not found: {context.MediaId}");
            }

            // Get OCR data from context or media entity
            var ocrData = context.Metadata.ContainsKey("OcrData")
                ? context.Metadata["OcrData"] as string
                : media.OcrPayloadJson;

            if (string.IsNullOrWhiteSpace(ocrData))
            {
                return MediaProcessingResult.Failed("OCR data not available for classification");
            }

            // Classify media type
            var mediaType = await _classificationService.ClassifyAsync(
                ocrData,
                context.ImageUri,
                cancellationToken);

            // Update media entity with classification
            media.Classify(mediaType, "system", DateTime.UtcNow);

            // Save changes
            await _mediaRepository.UpdateAsync(media, cancellationToken);

            // Store classification result in context
            context.Metadata["MediaType"] = mediaType;

            if (mediaType == MediaType.Unknown)
            {
                _logger.LogWarning(
                    "Media classification resulted in Unknown type for MediaId: {MediaId}. Status set to PendingClassification",
                    context.MediaId);
            }
            else
            {
                _logger.LogInformation(
                    "Media classified as {MediaType} for MediaId: {MediaId}",
                    mediaType.Value,
                    context.MediaId);
            }

            return MediaProcessingResult.Successful();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media classification failed for MediaId: {MediaId}", context.MediaId);
            return MediaProcessingResult.Failed($"Media classification failed: {ex.Message}", ex);
        }
    }
}
