using System.Text.Json;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Domain.Entities;

public sealed class Media
{
    public Guid Id { get; private set; }
    public int StoreId { get; private set; }
    public MediaType Type { get; private set; }
    public MediaStatus Status { get; private set; }
    public Uri ImageUri { get; private set; }
    public string? OcrPayloadJson { get; private set; }
    public string? Title { get; private set; }
    public string? Author { get; private set; }
    public string? Description { get; private set; }
    public decimal? Price { get; private set; }
    public AuditMetadata Audit { get; private set; }
    /// <summary>
    /// True if the media image has been classified as explicit/pornographic content.
    /// </summary>
    public bool IsExplicitContent { get; private set; }

    private Media(Guid id, int storeId, MediaType type, MediaStatus status, Uri imageUri, string? ocrPayloadJson, AuditMetadata audit)
    {
        Id = id;
        StoreId = storeId;
        Type = type;
        Status = status;
        ImageUri = imageUri;
        OcrPayloadJson = ocrPayloadJson;
        Audit = audit;
    }

    public static Media Create(int storeId, Uri imageUri, string createdBy, DateTime nowUtc)
    {
        // Validate StoreId
        if (storeId <= 0) throw new DomainValidationException("StoreId is required and must be a valid positive integer");

        // Validate ImageUri
        if (imageUri is null) throw new DomainValidationException("ImageUri is required");
        if (!imageUri.IsAbsoluteUri) throw new DomainValidationException("Image URI must be absolute");

        return new Media(
            Guid.NewGuid(),
            storeId,
            MediaType.Unknown,
            MediaStatus.Uploaded,
            imageUri,
            null,
            AuditMetadata.Create(createdBy, nowUtc));
    }

    public void StartProcessing(string updatedBy, DateTime nowUtc)
    {
        if (Status != MediaStatus.Uploaded)
            throw new DomainValidationException($"Cannot start processing media in status: {Status}");

        Status = MediaStatus.Processing;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    public void SetOcrData(string ocrJson, string updatedBy, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(ocrJson))
            throw new DomainValidationException("OcrPayloadJson is required");

        ValidateJson(ocrJson);
        OcrPayloadJson = ocrJson;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    public void Classify(MediaType mediaType, string updatedBy, DateTime nowUtc)
    {
        if (mediaType is null)
            throw new DomainValidationException("MediaType is required");

        Type = mediaType;

        // If classification fails, pend for manual review
        if (mediaType == MediaType.Unknown)
        {
            Status = MediaStatus.PendingClassification;
        }

        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    public void SetMetadata(string? title, string? author, string? description, decimal? price, string updatedBy, DateTime nowUtc)
    {
        Title = title?.Trim();
        Author = author?.Trim();
        Description = description?.Trim();

        if (price.HasValue && price.Value < 0)
            throw new DomainValidationException("Price cannot be negative");

        Price = price;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    public void ListInCatalog(string updatedBy, DateTime nowUtc)
    {
        if (Status == MediaStatus.PendingClassification)
            throw new DomainValidationException("Cannot list media pending classification");

        if (IsExplicitContent)
            throw new DomainValidationException("Cannot list explicit content");

        if (Type == MediaType.Unknown)
            throw new DomainValidationException("Cannot list media with unknown type");

        Status = MediaStatus.Listed;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    public void MarkAsFailed(string updatedBy, DateTime nowUtc)
    {
        Status = MediaStatus.Failed;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    /// <summary>
    /// Marks this media as explicit content. Typically called after external classification confirms explicit material.
    /// Idempotent: repeated calls have no further effect.
    /// </summary>
    public void FlagAsExplicitContent(string updatedBy, DateTime nowUtc)
    {
        if (IsExplicitContent) return; // idempotent
        IsExplicitContent = true;
        Status = MediaStatus.Flagged;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    /// <summary>
    /// Validates that a string is well-formed JSON by attempting to parse it.
    /// </summary>
    private static void ValidateJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new DomainValidationException($"OcrPayloadJson is not valid JSON: {ex.Message}", ex);
        }
    }
}
