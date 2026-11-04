namespace ThriftMedia.Domain.Events;

public record MediaProcessingFailedEvent(Guid MediaId, int StoreId, string Reason, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);
