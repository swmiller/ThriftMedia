namespace ThriftMedia.Domain.ValueObjects;

public sealed class MediaType : ValueObject
{
    public string Value { get; }

    private MediaType(string value) => Value = value;

    public static readonly MediaType Image = new("image");
    public static readonly MediaType Pdf = new("pdf");
    public static readonly MediaType Receipt = new("receipt");
    public static readonly MediaType Other = new("other");
    public static readonly MediaType Unknown = new("unknown");

    public static IEnumerable<MediaType> All => new[] { Image, Pdf, Receipt, Other, Unknown };

    public static MediaType From(string value)
    {
        value = (value ?? string.Empty).Trim().ToLowerInvariant();
        return All.FirstOrDefault(t => t.Value == value) ?? new MediaType(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
