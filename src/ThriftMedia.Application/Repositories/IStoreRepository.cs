using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Application.Repositories;

/// <summary>
/// Repository interface for Store entity persistence.
/// </summary>
public interface IStoreRepository
{
    Task<Store?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Store>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Store store, CancellationToken cancellationToken = default);
}
