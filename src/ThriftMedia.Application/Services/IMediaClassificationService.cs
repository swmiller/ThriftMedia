using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Application.Services;

/// <summary>
/// Service interface for classifying media type based on OCR data and image analysis.
/// </summary>
public interface IMediaClassificationService
{
    /// <summary>
    /// Classifies the media type based on OCR data extracted from the image.
    /// </summary>
    /// <param name="ocrJsonData">JSON string containing OCR results.</param>
    /// <param name="imageUri">URI of the media image.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The classified MediaType.</returns>
    Task<MediaType> ClassifyAsync(
        string ocrJsonData,
        Uri imageUri,
        CancellationToken cancellationToken = default);
}
