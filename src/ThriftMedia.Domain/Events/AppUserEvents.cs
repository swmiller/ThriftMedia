namespace ThriftMedia.Domain.Events;

public record AppUserSuspendedEvent(int AppUserId, string Email, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);

public record AppUserReactivatedEvent(int AppUserId, string Email, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);
