using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Repositories;
using ThriftMedia.Application.Services;

namespace ThriftMedia.Application.Pipelines.Steps;

/// <summary>
/// Processing step that performs content moderation to detect explicit content.
/// If explicit content is detected, flags the media and disables the store's catalog.
/// </summary>
public class ContentModerationStep : IMediaProcessingStep
{
    private readonly IContentModerationService _moderationService;
    private readonly IMediaRepository _mediaRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<ContentModerationStep> _logger;

    public string StepName => "Content Moderation";

    public ContentModerationStep(
        IContentModerationService moderationService,
        IMediaRepository mediaRepository,
        IStoreRepository storeRepository,
        ILogger<ContentModerationStep> logger)
    {
        _moderationService = moderationService ?? throw new ArgumentNullException(nameof(moderationService));
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting content moderation for MediaId: {MediaId}", context.MediaId);

            // Load media entity
            var media = await _mediaRepository.GetByIdAsync(context.MediaId, cancellationToken);
            if (media == null)
            {
                return MediaProcessingResult.Failed($"Media not found: {context.MediaId}");
            }

            // Perform content moderation
            var isExplicit = await _moderationService.IsExplicitContentAsync(
                context.ImageUri,
                cancellationToken);

            if (isExplicit)
            {
                _logger.LogWarning(
                    "Explicit content detected for MediaId: {MediaId}, StoreId: {StoreId}",
                    context.MediaId,
                    context.StoreId);

                // Flag the media as explicit
                media.FlagAsExplicitContent("system", DateTime.UtcNow);
                await _mediaRepository.UpdateAsync(media, cancellationToken);

                // Flag the store to disable catalog
                var store = await _storeRepository.GetByIdAsync(context.StoreId, cancellationToken);
                if (store != null)
                {
                    store.FlagExplicitContent("system", DateTime.UtcNow);
                    await _storeRepository.UpdateAsync(store, cancellationToken);

                    _logger.LogWarning(
                        "Store catalog disabled for StoreId: {StoreId} due to explicit content",
                        context.StoreId);
                }

                // Store moderation result in context
                context.Metadata["IsExplicitContent"] = true;

                // Return success - the pipeline completed but media is flagged
                return MediaProcessingResult.Successful();
            }

            _logger.LogInformation(
                "No explicit content detected for MediaId: {MediaId}",
                context.MediaId);

            context.Metadata["IsExplicitContent"] = false;

            return MediaProcessingResult.Successful();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content moderation failed for MediaId: {MediaId}", context.MediaId);
            return MediaProcessingResult.Failed($"Content moderation failed: {ex.Message}", ex);
        }
    }
}
