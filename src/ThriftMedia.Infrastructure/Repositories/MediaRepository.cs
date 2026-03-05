using Microsoft.EntityFrameworkCore;
using ThriftMedia.Application.Repositories;
using DomainMedia = ThriftMedia.Domain.Entities.Media;
using DomainMediaType = ThriftMedia.Domain.ValueObjects.MediaType;
using DomainMediaStatus = ThriftMedia.Domain.ValueObjects.MediaStatus;
using PersistenceMedia = ThriftMedia.Infrastructure.Persistence.Models.Media;
using ThriftMedia.Infrastructure.Persistence.Models;

namespace ThriftMedia.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Media entity.
/// Maps between Domain entities and Persistence models.
/// </summary>
public class MediaRepository : IMediaRepository
{
    private readonly ThriftMediaDbContext _context;

    public MediaRepository(ThriftMediaDbContext context)
    {
        _context = context;
    }

    public async Task<DomainMedia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.MediaItems.FindAsync(new object[] { id }, cancellationToken);
        return model == null ? null : ToDomain(model);
    }

    public async Task<IEnumerable<DomainMedia>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var models = await _context.MediaItems.AsNoTracking().ToListAsync(cancellationToken);
        return models.Select(ToDomain);
    }

    public async Task<DomainMedia> AddAsync(DomainMedia media, CancellationToken cancellationToken = default)
    {
        var model = ToModel(media);
        await _context.MediaItems.AddAsync(model, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDomain(model);
    }

    public async Task UpdateAsync(DomainMedia media, CancellationToken cancellationToken = default)
    {
        var model = await _context.MediaItems.FindAsync(new object[] { media.Id }, cancellationToken);
        if (model != null)
        {
            UpdateModel(model, media);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<DomainMedia>> GetByStoreIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        var models = await _context.MediaItems
            .Where(m => m.StoreId == storeId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return models.Select(ToDomain);
    }

    // Mapping methods (temporary until Phase 3 impedance mismatch is resolved)
    private static DomainMedia ToDomain(PersistenceMedia model)
    {
        // Parse Type - note: MediaType is a value object, not enum
        var typeStr = model.Type?.ToLowerInvariant() ?? "unknown";
        var mediaType = typeStr switch
        {
            "book" => DomainMediaType.Book,
            "video" => DomainMediaType.Video,
            "cdrom" => DomainMediaType.CDRom,
            "vinyl-record" => DomainMediaType.VinylRecord,
            "eight-track" => DomainMediaType.EightTrack,
            "cassette" => DomainMediaType.Cassette,
            "dvd" => DomainMediaType.DVD,
            "blu-ray" => DomainMediaType.BluRay,
            "magazine" => DomainMediaType.Magazine,
            "comic" => DomainMediaType.Comic,
            "other" => DomainMediaType.Other,
            _ => DomainMediaType.Unknown
        };

        var media = DomainMedia.Create(
            model.StoreId,
            new Uri(model.ImageUri),
            model.CreatedBy,
            model.CreatedAt
        );

        // Use reflection to set private Id field (temporary workaround)
        typeof(DomainMedia).GetProperty("Id")!.SetValue(media, model.Id);

        if (!string.IsNullOrEmpty(model.OcrPayloadJson))
        {
            media.SetOcrData(model.OcrPayloadJson, model.UpdatedBy ?? "system", model.UpdatedAt ?? DateTime.UtcNow);
        }
        if (mediaType != DomainMediaType.Unknown)
        {
            media.Classify(mediaType, model.UpdatedBy ?? "system", model.UpdatedAt ?? DateTime.UtcNow);
        }
        if (model.Title != null || model.Author != null || model.Description != null || model.Price.HasValue)
        {
            media.SetMetadata(model.Title, model.Author, model.Description, model.Price,
                model.UpdatedBy ?? "system", model.UpdatedAt ?? DateTime.UtcNow);
        }
        if (model.IsExplicitContent)
        {
            media.FlagAsExplicitContent(model.UpdatedBy ?? "system", model.UpdatedAt ?? DateTime.UtcNow);
        }

        return media;
    }

    private static PersistenceMedia ToModel(DomainMedia domain)
    {
        return new PersistenceMedia
        {
            Id = domain.Id,
            StoreId = domain.StoreId,
            Title = domain.Title,
            Author = domain.Author,
            Description = domain.Description,
            Price = domain.Price,
            Type = domain.Type?.ToString() ?? "Unknown",
            Status = domain.Status.ToString(),
            ImageUri = domain.ImageUri?.ToString() ?? string.Empty,
            OcrPayloadJson = domain.OcrPayloadJson,
            IsExplicitContent = domain.IsExplicitContent,
            CreatedAt = domain.Audit.CreatedAtUtc,
            CreatedBy = domain.Audit.CreatedBy,
            UpdatedAt = domain.Audit.UpdatedAtUtc,
            UpdatedBy = domain.Audit.UpdatedBy
        };
    }

    private static void UpdateModel(PersistenceMedia model, DomainMedia domain)
    {
        model.Title = domain.Title;
        model.Author = domain.Author;
        model.Description = domain.Description;
        model.Price = domain.Price;
        model.Type = domain.Type?.ToString() ?? "Unknown";
        model.Status = domain.Status.ToString();
        model.ImageUri = domain.ImageUri?.ToString() ?? string.Empty;
        model.OcrPayloadJson = domain.OcrPayloadJson;
        model.IsExplicitContent = domain.IsExplicitContent;
        model.UpdatedAt = domain.Audit.UpdatedAtUtc;
        model.UpdatedBy = domain.Audit.UpdatedBy;
    }
}
