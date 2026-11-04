namespace ThriftMedia.Contracts.Requests;

/// <summary>The reason is included in the rejection email sent to the applicant.</summary>
public record RejectStoreApplicationRequest(string? Reason);
