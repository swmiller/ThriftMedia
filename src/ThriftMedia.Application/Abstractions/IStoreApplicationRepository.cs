using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Application.Abstractions;

public interface IStoreApplicationRepository
{
    Task<StoreApplication?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Pending applications, oldest first.</summary>
    Task<IReadOnlyList<StoreApplication>> GetPendingAsync(CancellationToken cancellationToken);

    Task<bool> HasPendingForEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Inserts the application and returns the generated id.</summary>
    Task<int> AddAsync(StoreApplication application, CancellationToken cancellationToken);

    Task UpdateAsync(StoreApplication application, CancellationToken cancellationToken);
}
