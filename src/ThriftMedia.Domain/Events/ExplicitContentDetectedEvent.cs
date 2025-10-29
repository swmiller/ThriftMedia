namespace ThriftMedia.Domain.Events;

public record ExplicitContentDetectedEvent(Guid MediaId, int StoreId) : DomainEvent;
