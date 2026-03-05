using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Application.Repositories;

/// <summary>
/// Repository interface for Media entity persistence.
/// </summary>
public interface IMediaRepository
{
    Task<Media?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Media>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Media> AddAsync(Media media, CancellationToken cancellationToken = default);
    Task UpdateAsync(Media media, CancellationToken cancellationToken = default);
    Task<IEnumerable<Media>> GetByStoreIdAsync(int storeId, CancellationToken cancellationToken = default);
}
