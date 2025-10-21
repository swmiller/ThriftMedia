namespace ThriftMedia.Domain.ValueObjects;

public sealed class AuditMetadata : ValueObject
{
    public string CreatedBy { get; }
    public DateTime CreatedAtUtc { get; }
    public string? UpdatedBy { get; }
    public DateTime? UpdatedAtUtc { get; }

    private AuditMetadata(string createdBy, DateTime createdAtUtc, string? updatedBy, DateTime? updatedAtUtc)
    {
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = updatedAtUtc;
    }

    public static AuditMetadata Create(string createdBy, DateTime nowUtc) => new(createdBy, nowUtc, null, null);

    public AuditMetadata WithUpdated(string user, DateTime nowUtc) => new(CreatedBy, CreatedAtUtc, user, nowUtc);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CreatedBy;
        yield return CreatedAtUtc;
        yield return UpdatedBy;
        yield return UpdatedAtUtc;
    }
}
