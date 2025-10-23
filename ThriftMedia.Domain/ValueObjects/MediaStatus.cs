namespace ThriftMedia.Domain.ValueObjects;

public sealed class MediaStatus : ValueObject
{
    public string Value { get; }

    private MediaStatus(string value) => Value = value;

    public static readonly MediaStatus Uploaded = new("uploaded");
    public static readonly MediaStatus Processing = new("processing");
    public static readonly MediaStatus PendingClassification = new("pending-classification");
    public static readonly MediaStatus Flagged = new("flagged");
    public static readonly MediaStatus Listed = new("listed");
    public static readonly MediaStatus Failed = new("failed");

    public static IEnumerable<MediaStatus> All => new[]
    {
        Uploaded, Processing, PendingClassification, Flagged, Listed, Failed
    };

    public static MediaStatus From(string value)
    {
        value = (value ?? string.Empty).Trim().ToLowerInvariant();
        return All.FirstOrDefault(s => s.Value == value) ?? throw new ArgumentException($"Invalid media status: {value}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
