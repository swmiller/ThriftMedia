using ThriftMedia.Application.Services;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Infrastructure.Services;

/// <summary>
/// Implementation of media classification service.
/// TODO: Integrate with actual AI classification provider (e.g., Azure AI Vision, Custom ML model).
/// </summary>
public class MediaClassificationService : IMediaClassificationService
{
    public Task<MediaType> ClassifyAsync(
        string ocrJsonData,
        Uri imageUri,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual classification logic based on OCR data and image analysis
        // For now, return default Book type
        return Task.FromResult(MediaType.Book);
    }
}
