using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Application.Abstractions;

public interface IAppUserRepository
{
    Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<AppUser?> GetByProviderAsync(string provider, string providerSub, CancellationToken cancellationToken);

    /// <summary>Case-insensitive email lookup.</summary>
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>The single System Admin row, if it exists yet.</summary>
    Task<AppUser?> GetSystemAdminAsync(CancellationToken cancellationToken);

    /// <summary>Inserts the user and returns the generated id.</summary>
    Task<int> AddAsync(AppUser user, CancellationToken cancellationToken);

    Task UpdateAsync(AppUser user, CancellationToken cancellationToken);
}
