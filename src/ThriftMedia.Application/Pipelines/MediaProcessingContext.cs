namespace ThriftMedia.Application.Pipelines;

/// <summary>
/// Context object passed through the media processing pipeline containing metadata and state.
/// </summary>
public class MediaProcessingContext
{
    public Guid MediaId { get; init; }
    public int StoreId { get; init; }
    public Uri ImageUri { get; init; }

    /// <summary>
    /// Extensible metadata bag for passing data between pipeline steps.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    public MediaProcessingContext(Guid mediaId, int storeId, Uri imageUri)
    {
        MediaId = mediaId;
        StoreId = storeId;
        ImageUri = imageUri;
    }
}
