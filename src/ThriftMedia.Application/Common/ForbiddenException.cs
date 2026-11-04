namespace ThriftMedia.Application.Common;

/// <summary>Thrown when the caller is authenticated but not permitted to perform the operation. Maps to HTTP 403.</summary>
public sealed class ForbiddenException(string message) : Exception(message);
