using Microsoft.EntityFrameworkCore;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Domain.Services;

namespace ThriftMedia.Infrastructure.Services;

/// <summary>
/// Implementation of media moderation service that checks for explicit content across store media.
/// </summary>
public class MediaModerationService : IMediaModerationService
{
    private readonly ThriftMediaDbContext _context;

    public MediaModerationService(ThriftMediaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> HasExplicitContentAsync(int storeId, CancellationToken cancellationToken = default)
    {
        return await _context.MediaItems
            .Where(m => m.StoreId == storeId && m.IsExplicitContent)
            .AnyAsync(cancellationToken);
    }
}
