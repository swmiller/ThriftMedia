namespace ThriftMedia.Domain.Services;

/// <summary>
/// Domain service for checking explicit content across media items associated with a store.
/// </summary>
public interface IMediaModerationService
{
    /// <summary>
    /// Checks if any media items associated with the given store have explicit content flagged.
    /// </summary>
    /// <param name="storeId">The store ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if any media item has IsExplicitContent = true.</returns>
    Task<bool> HasExplicitContentAsync(int storeId, CancellationToken cancellationToken = default);
}
