namespace ThriftMedia.Domain.ValueObjects;

public sealed class MediaType : ValueObject
{
    public string Value { get; }

    private MediaType(string value) => Value = value;

    public static readonly MediaType Book = new("book");
    public static readonly MediaType Video = new("video");
    public static readonly MediaType CDRom = new("cdrom");
    public static readonly MediaType VinylRecord = new("vinyl-record");
    public static readonly MediaType EightTrack = new("eight-track");
    public static readonly MediaType Cassette = new("cassette");
    public static readonly MediaType DVD = new("dvd");
    public static readonly MediaType BluRay = new("blu-ray");
    public static readonly MediaType Magazine = new("magazine");
    public static readonly MediaType Comic = new("comic");
    public static readonly MediaType Other = new("other");
    public static readonly MediaType Unknown = new("unknown");

    public static IEnumerable<MediaType> All => new[]
    {
        Book, Video, CDRom, VinylRecord, EightTrack, Cassette,
        DVD, BluRay, Magazine, Comic, Other, Unknown
    };

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
