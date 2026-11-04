namespace ThriftMedia.Domain.Events;

public record StoreApplicationApprovedEvent(int ApplicationId, string ApplicantEmail, string StoreName, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);

public record StoreApplicationRejectedEvent(int ApplicationId, string ApplicantEmail, string StoreName, string? Reason, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);
