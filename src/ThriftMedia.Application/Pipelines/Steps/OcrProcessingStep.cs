using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Repositories;
using ThriftMedia.Application.Services;

namespace ThriftMedia.Application.Pipelines.Steps;

/// <summary>
/// Processing step that performs OCR on the media image.
/// </summary>
public class OcrProcessingStep : IMediaProcessingStep
{
    private readonly IOcrService _ocrService;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<OcrProcessingStep> _logger;

    public string StepName => "OCR Processing";

    public OcrProcessingStep(
        IOcrService ocrService,
        IMediaRepository mediaRepository,
        ILogger<OcrProcessingStep> logger)
    {
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting OCR processing for MediaId: {MediaId}", context.MediaId);

            // Load media entity
            var media = await _mediaRepository.GetByIdAsync(context.MediaId, cancellationToken);
            if (media == null)
            {
                return MediaProcessingResult.Failed($"Media not found: {context.MediaId}");
            }

            // Perform OCR
            var ocrJsonData = await _ocrService.ProcessImageAsync(context.ImageUri, cancellationToken);

            // Update media entity with OCR data
            media.SetOcrData(ocrJsonData, "system", DateTime.UtcNow);

            // Save changes
            await _mediaRepository.UpdateAsync(media, cancellationToken);

            // Store OCR data in context for next steps
            context.Metadata["OcrData"] = ocrJsonData;

            _logger.LogInformation("OCR processing completed for MediaId: {MediaId}", context.MediaId);

            return MediaProcessingResult.Successful();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed for MediaId: {MediaId}", context.MediaId);
            return MediaProcessingResult.Failed($"OCR processing failed: {ex.Message}", ex);
        }
    }
}
