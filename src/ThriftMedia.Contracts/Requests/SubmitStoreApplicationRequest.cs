namespace ThriftMedia.Contracts.Requests;

/// <summary>
/// Public "Apply" form. <see cref="LicenseImageRef"/> and <see cref="StoreFrontImageRef"/> are references to
/// already-stored uploads (file storage is designed separately).
/// </summary>
public record SubmitStoreApplicationRequest(
    string StoreName,
    string OwnerFullName,
    string Email,
    string PhoneNumber,
    string Address1,
    string? Address2,
    string City,
    string ProvinceState,
    string PostalCode,
    string? Country,
    string? WebsiteUrl,
    string Description,
    string LicenseNumber,
    string LicenseImageRef,
    string? StoreFrontImageRef);
