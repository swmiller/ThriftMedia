namespace ThriftMedia.Application.Services;

/// <summary>
/// Service interface for OCR (Optical Character Recognition) operations.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Processes an image and extracts text content using OCR.
    /// </summary>
    /// <param name="imageUri">URI of the image to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON string containing OCR results.</returns>
    Task<string> ProcessImageAsync(Uri imageUri, CancellationToken cancellationToken = default);
}
