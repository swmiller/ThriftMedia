using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Repositories;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Application.Pipelines.Steps;

/// <summary>
/// Processing step that lists media in the catalog if all validation passes.
/// </summary>
public class CatalogListingStep : IMediaProcessingStep
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<CatalogListingStep> _logger;

    public string StepName => "Catalog Listing";

    public CatalogListingStep(
        IMediaRepository mediaRepository,
        ILogger<CatalogListingStep> logger)
    {
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting catalog listing for MediaId: {MediaId}", context.MediaId);

            // Load media entity
            var media = await _mediaRepository.GetByIdAsync(context.MediaId, cancellationToken);
            if (media == null)
            {
                return MediaProcessingResult.Failed($"Media not found: {context.MediaId}");
            }

            // Check if media is explicit content
            var isExplicit = context.Metadata.ContainsKey("IsExplicitContent") &&
                           (bool)context.Metadata["IsExplicitContent"];

            if (isExplicit || media.IsExplicitContent)
            {
                _logger.LogWarning(
                    "Cannot list explicit content in catalog for MediaId: {MediaId}",
                    context.MediaId);

                // Media is flagged, do not list in catalog
                return MediaProcessingResult.Successful();
            }

            // Check if media type is unknown (pending classification)
            if (media.Type == MediaType.Unknown)
            {
                _logger.LogWarning(
                    "Cannot list media with unknown type in catalog for MediaId: {MediaId}. Awaiting manual classification.",
                    context.MediaId);

                // Media needs manual classification, do not list yet
                return MediaProcessingResult.Successful();
            }

            // List media in catalog
            media.ListInCatalog("system", DateTime.UtcNow);
            await _mediaRepository.UpdateAsync(media, cancellationToken);

            _logger.LogInformation(
                "Media successfully listed in catalog for MediaId: {MediaId}",
                context.MediaId);

            return MediaProcessingResult.Successful();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Catalog listing failed for MediaId: {MediaId}", context.MediaId);
            return MediaProcessingResult.Failed($"Catalog listing failed: {ex.Message}", ex);
        }
    }
}
