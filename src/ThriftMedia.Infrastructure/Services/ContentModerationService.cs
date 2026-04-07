using ThriftMedia.Application.Services;

namespace ThriftMedia.Infrastructure.Services;

/// <summary>
/// Implementation of content moderation service.
/// TODO: Integrate with actual content moderation provider (e.g., Azure Content Safety).
/// </summary>
public class ContentModerationService : IContentModerationService
{
    public Task<bool> IsExplicitContentAsync(Uri imageUri, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual content moderation
        // For now, return false (no explicit content detected)
        return Task.FromResult(false);
    }
}
