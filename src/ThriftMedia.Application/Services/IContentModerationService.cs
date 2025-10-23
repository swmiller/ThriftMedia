namespace ThriftMedia.Application.Services;

/// <summary>
/// Service interface for content moderation operations.
/// Analyzes media images for explicit or inappropriate content.
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Analyzes an image for explicit or adult content.
    /// </summary>
    /// <param name="imageUri">URI of the image to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if explicit content is detected, otherwise false.</returns>
    Task<bool> IsExplicitContentAsync(Uri imageUri, CancellationToken cancellationToken = default);
}
