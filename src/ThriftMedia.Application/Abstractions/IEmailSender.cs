namespace ThriftMedia.Application.Abstractions;

public record EmailMessage(string To, string Subject, string Body);

/// <summary>Platform email abstraction. The initial implementation only logs; a real provider replaces it later.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
