namespace ThriftMedia.Domain.Events;

public record MediaListedEvent(Guid MediaId, int StoreId) : DomainEvent;
