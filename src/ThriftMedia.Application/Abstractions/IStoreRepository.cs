using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Application.Abstractions;

/// <summary>Minimal Store persistence needed by the auth slices; grows when the Store handlers are reimplemented with Dapper.</summary>
public interface IStoreRepository
{
    /// <summary>Inserts the store and returns the generated id.</summary>
    Task<int> AddAsync(Store store, CancellationToken cancellationToken);

    /// <summary>The id of the store owned by the given Store Admin, or null if none.</summary>
    Task<int?> GetIdByAppUserIdAsync(int appUserId, CancellationToken cancellationToken);
}
