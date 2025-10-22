using Microsoft.EntityFrameworkCore;
using ThriftMedia.Data;
using ThriftMedia.Domain.Services;

namespace ThriftMedia.Infrastructure.Services;

/// <summary>
/// Implementation of media moderation service that checks for explicit content across store media.
/// </summary>
public class MediaModerationService : IMediaModerationService
{
    private readonly ApplicationDbContext _context;

    public MediaModerationService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> HasExplicitContentAsync(int storeId, CancellationToken cancellationToken = default)
    {
        return await _context.Media
            .Where(m => m.StoreId == storeId && m.IsExplicitContent)
            .AnyAsync(cancellationToken);
    }
}
