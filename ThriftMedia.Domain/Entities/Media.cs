using System.Text.Json;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Domain.Entities;

public sealed class Media
{
    public int Id { get; private set; } // 0 until persisted
    public int StoreId { get; private set; }
    public MediaType Type { get; private set; }
    public Uri ImageUri { get; private set; }
    public string? OcrPayloadJson { get; private set; }
    public AuditMetadata Audit { get; private set; }
    /// <summary>
    /// True if the media image has been classified as explicit/pornographic content.
    /// </summary>
    public bool IsExplicitContent { get; private set; }

    private Media(int storeId, MediaType type, Uri imageUri, string? ocrPayloadJson, AuditMetadata audit)
    {
        StoreId = storeId;
        Type = type;
        ImageUri = imageUri;
        OcrPayloadJson = ocrPayloadJson;
        Audit = audit;
    }

    public static Media Create(int storeId, MediaType type, Uri imageUri, string? ocrJson, string createdBy, DateTime nowUtc)
    {
        // Validate StoreId
        if (storeId <= 0) throw new DomainValidationException("StoreId is required and must be a valid positive integer");

        // Validate MediaType
        if (type is null) throw new DomainValidationException("MediaType is required");

        // Validate ImageUri
        if (imageUri is null) throw new DomainValidationException("ImageUri is required");
        if (!imageUri.IsAbsoluteUri) throw new DomainValidationException("Image URI must be absolute");

        // Validate OcrPayloadJson
        if (string.IsNullOrWhiteSpace(ocrJson)) throw new DomainValidationException("OcrPayloadJson is required");
        ValidateJson(ocrJson!);

        return new Media(storeId, type, imageUri, ocrJson, AuditMetadata.Create(createdBy, nowUtc));
    }

    public void UpdateOcr(string? ocrJson, string updatedBy, DateTime nowUtc)
    {
        // Validate OcrPayloadJson is required
        if (string.IsNullOrWhiteSpace(ocrJson)) throw new DomainValidationException("OcrPayloadJson is required");
        ValidateJson(ocrJson!);

        OcrPayloadJson = ocrJson;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    /// <summary>
    /// Stub for future content moderation service integration.
    /// Will analyze the ImageUri content to detect explicit/pornographic material.
    /// Currently returns false (no explicit content detected).
    /// </summary>
    public bool ClassifyContent()
    {
        // TODO: Integrate content moderation API (e.g., Azure Content Moderator, AWS Rekognition, local ML model)
        return false;
    }

    /// <summary>
    /// Marks this media as explicit content. Typically called after external classification confirms explicit material.
    /// Idempotent: repeated calls have no further effect.
    /// </summary>
    public void FlagAsExplicitContent(string updatedBy, DateTime nowUtc)
    {
        if (IsExplicitContent) return; // idempotent
        IsExplicitContent = true;
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
