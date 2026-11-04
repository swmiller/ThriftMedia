namespace ThriftMedia.Domain.Events;

public record MediaListedEvent(Guid MediaId, int StoreId, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);
