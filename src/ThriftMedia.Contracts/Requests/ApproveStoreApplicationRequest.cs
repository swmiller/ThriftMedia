namespace ThriftMedia.Contracts.Requests;

/// <summary>License details the System Admin verified while reviewing the application.</summary>
public record ApproveStoreApplicationRequest(
    string LicenseType,
    string IssuingAuthority,
    DateTime IssueDate,
    DateTime? ExpirationDate,
    string? Notes);
