namespace ThriftMedia.Domain.Events;

public record MediaUploadedEvent(Guid MediaId, int StoreId, Uri ImageUri, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);
