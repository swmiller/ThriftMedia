namespace ThriftMedia.Contracts.Dto;

public record StoreApplicationDto(
    int Id,
    string Status,
    string StoreName,
    string OwnerFullName,
    string Email,
    string PhoneNumber,
    string Address1,
    string Address2,
    string City,
    string ProvinceState,
    string PostalCode,
    string Country,
    string? WebsiteUrl,
    string Description,
    string LicenseNumber,
    string LicenseImageRef,
    string? StoreFrontImageRef,
    DateTime SubmittedAtUtc,
    string? ReviewNotes);
