namespace ThriftMedia.Domain.Events;

public record MediaProcessingFailedEvent(Guid MediaId, int StoreId, string Reason) : DomainEvent;
