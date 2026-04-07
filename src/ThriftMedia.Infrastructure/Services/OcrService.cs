using ThriftMedia.Application.Services;

namespace ThriftMedia.Infrastructure.Services;

/// <summary>
/// Implementation of OCR service.
/// TODO: Integrate with actual OCR provider (e.g., Azure AI Vision, Tesseract).
/// </summary>
public class OcrService : IOcrService
{
    public Task<string> ProcessImageAsync(Uri imageUri, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual OCR processing
        // For now, return empty JSON result
        return Task.FromResult("{}");
    }
}
