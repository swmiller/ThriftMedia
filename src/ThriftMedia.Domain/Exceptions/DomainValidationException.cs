namespace ThriftMedia.Domain.Exceptions;

public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
    public DomainValidationException(string message, Exception inner) : base(message, inner) { }
}
