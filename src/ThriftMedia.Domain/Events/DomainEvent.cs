namespace ThriftMedia.Domain.Events;

public abstract record DomainEvent(DateTime OccurredAtUtc)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
