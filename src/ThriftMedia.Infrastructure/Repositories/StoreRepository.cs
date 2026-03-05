using Microsoft.EntityFrameworkCore;
using ThriftMedia.Application.Repositories;
using DomainStore = ThriftMedia.Domain.Entities.Store;
using DomainAddress = ThriftMedia.Domain.ValueObjects.Address;
using PersistenceStore = ThriftMedia.Infrastructure.Persistence.Models.Store;
using PersistenceAddress = ThriftMedia.Infrastructure.Persistence.Models.Address;
using ThriftMedia.Infrastructure.Persistence.Models;

namespace ThriftMedia.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Store entity.
/// Maps between Domain entities and Persistence models.
/// </summary>
public class StoreRepository : IStoreRepository
{
    private readonly ThriftMediaDbContext _context;

    public StoreRepository(ThriftMediaDbContext context)
    {
        _context = context;
    }

    public async Task<DomainStore?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Stores.FindAsync(new object[] { id }, cancellationToken);
        return model == null ? null : ToDomain(model);
    }

    public async Task<IEnumerable<DomainStore>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var models = await _context.Stores.AsNoTracking().ToListAsync(cancellationToken);
        return models.Select(ToDomain);
    }

    public async Task UpdateAsync(DomainStore store, CancellationToken cancellationToken = default)
    {
        var model = await _context.Stores.FindAsync(new object[] { store.Id }, cancellationToken);
        if (model != null)
        {
            UpdateModel(model, store);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Mapping methods (temporary until Phase 3 impedance mismatch is resolved)
    private static DomainStore ToDomain(PersistenceStore model)
    {
        var address = model.Address != null
            ? DomainAddress.Create(
                model.Address.Street ?? string.Empty,
                model.Address.City ?? string.Empty,
                model.Address.State ?? string.Empty,
                model.Address.ZipCode ?? string.Empty,
                model.Address.Country ?? "US"
              )
            : DomainAddress.Create(string.Empty, string.Empty, string.Empty, string.Empty, "US");

        var store = DomainStore.Create(
            model.Name,
            address,
            model.CreatedBy,
            model.CreatedAt
        );

        // Use reflection to set Id (temporary workaround)
        typeof(DomainStore).GetProperty("Id")!.SetValue(store, model.Id);

        if (model.BusinessLicenseImageUri != null)
        {
            store.SetBusinessLicenseImage(
                new Uri(model.BusinessLicenseImageUri),
                model.UpdatedBy ?? "system",
                model.UpdatedAt ?? DateTime.UtcNow
            );
        }

        if (model.ExplicitContentFlagged)
        {
            store.FlagExplicitContent(model.UpdatedBy ?? "system", model.UpdatedAt ?? DateTime.UtcNow);
        }

        return store;
    }

    private static void UpdateModel(PersistenceStore model, DomainStore domain)
    {
        model.Name = domain.Name;
        model.BusinessLicenseImageUri = domain.BusinessLicenseImageUri?.ToString();
        model.ExplicitContentFlagged = domain.ExplicitContentFlagged;
        model.UpdatedAt = domain.Audit.UpdatedAtUtc;
        model.UpdatedBy = domain.Audit.UpdatedBy;

        if (domain.Address != null)
        {
            if (model.Address == null)
            {
                model.Address = new PersistenceAddress();
            }
            model.Address.Street = domain.Address.Street;
            model.Address.City = domain.Address.City;
            model.Address.State = domain.Address.State;
            model.Address.ZipCode = domain.Address.ZipCode;
            model.Address.Country = domain.Address.Country;
        }
    }
}
