using ThriftMedia.Domain.Enums;
using ThriftMedia.Domain.Events;
using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Domain.Entities;

/// <summary>
/// A prospective store owner's request for a Store Admin account.
/// Lifecycle: Pending to Approved, or Pending to Rejected. Both outcomes are terminal;
/// suspension and reactivation apply to the resulting <see cref="AppUser"/>.
/// </summary>
public sealed class StoreApplication : Entity
{
    public const string RequiredCountry = "United States";

    public int Id { get; private set; }
    public StoreApplicationStatus Status { get; private set; }
    public string StoreName { get; private set; } = string.Empty;
    public string OwnerFullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Address1 { get; private set; } = string.Empty;
    public string Address2 { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string ProvinceState { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = RequiredCountry;
    public string? WebsiteUrl { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string LicenseImageRef { get; private set; } = string.Empty;
    public string? StoreFrontImageRef { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public int? ReviewedByAppUserId { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public string? ReviewNotes { get; private set; }
    public int? AppUserId { get; private set; }

    private StoreApplication() { }

    public static StoreApplication Submit(
        string storeName, string ownerFullName, string email, string phoneNumber,
        string address1, string? address2, string city, string provinceState, string postalCode, string? country,
        string? websiteUrl, string description,
        string licenseNumber, string licenseImageRef, string? storeFrontImageRef,
        DateTime nowUtc)
    {
        var resolvedCountry = DomainGuard.Optional(country, nameof(Country), 100) ?? RequiredCountry;
        if (!string.Equals(resolvedCountry, RequiredCountry, StringComparison.OrdinalIgnoreCase))
            throw new DomainValidationException($"Country must be {RequiredCountry}");

        return new StoreApplication
        {
            Status = StoreApplicationStatus.Pending,
            StoreName = DomainGuard.Required(storeName, nameof(StoreName), 100),
            OwnerFullName = DomainGuard.Required(ownerFullName, nameof(OwnerFullName), 100),
            Email = DomainGuard.Email(email, nameof(Email)),
            PhoneNumber = DomainGuard.Required(phoneNumber, nameof(PhoneNumber), 50),
            Address1 = DomainGuard.Required(address1, nameof(Address1), 150),
            Address2 = DomainGuard.Optional(address2, nameof(Address2), 150) ?? string.Empty,
            City = DomainGuard.Required(city, nameof(City), 100),
            ProvinceState = DomainGuard.Required(provinceState, nameof(ProvinceState), 50),
            PostalCode = DomainGuard.Required(postalCode, nameof(PostalCode), 20),
            Country = RequiredCountry,
            WebsiteUrl = DomainGuard.HttpUrl(websiteUrl, nameof(WebsiteUrl), 255),
            Description = DomainGuard.Required(description, nameof(Description), 1000),
            LicenseNumber = DomainGuard.Required(licenseNumber, nameof(LicenseNumber), 100),
            LicenseImageRef = DomainGuard.Required(licenseImageRef, nameof(LicenseImageRef), 500),
            StoreFrontImageRef = DomainGuard.Optional(storeFrontImageRef, nameof(StoreFrontImageRef), 500),
            SubmittedAtUtc = nowUtc
        };
    }

    public static StoreApplication Rehydrate(
        int id, StoreApplicationStatus status,
        string storeName, string ownerFullName, string email, string phoneNumber,
        string address1, string address2, string city, string provinceState, string postalCode, string country,
        string? websiteUrl, string description,
        string licenseNumber, string licenseImageRef, string? storeFrontImageRef,
        DateTime submittedAtUtc, int? reviewedByAppUserId, DateTime? reviewedAtUtc, string? reviewNotes, int? appUserId) => new()
        {
            Id = DomainGuard.Positive(id, nameof(Id)),
            Status = status,
            StoreName = storeName,
            OwnerFullName = ownerFullName,
            Email = email,
            PhoneNumber = phoneNumber,
            Address1 = address1,
            Address2 = address2,
            City = city,
            ProvinceState = provinceState,
            PostalCode = postalCode,
            Country = country,
            WebsiteUrl = websiteUrl,
            Description = description,
            LicenseNumber = licenseNumber,
            LicenseImageRef = licenseImageRef,
            StoreFrontImageRef = storeFrontImageRef,
            SubmittedAtUtc = submittedAtUtc,
            ReviewedByAppUserId = reviewedByAppUserId,
            ReviewedAtUtc = reviewedAtUtc,
            ReviewNotes = reviewNotes,
            AppUserId = appUserId
        };

    /// <summary>
    /// Pending to Approved. <paramref name="storeAdminAppUserId"/> is the id of the StoreAdmin
    /// <see cref="AppUser"/> created for the applicant.
    /// </summary>
    public void Approve(int reviewerAppUserId, int storeAdminAppUserId, string? notes, DateTime nowUtc)
    {
        EnsurePending();
        ReviewedByAppUserId = DomainGuard.Positive(reviewerAppUserId, nameof(reviewerAppUserId));
        AppUserId = DomainGuard.Positive(storeAdminAppUserId, nameof(storeAdminAppUserId));
        ReviewNotes = DomainGuard.Optional(notes, nameof(ReviewNotes), 1000);
        ReviewedAtUtc = nowUtc;
        Status = StoreApplicationStatus.Approved;
        AddDomainEvent(new StoreApplicationApprovedEvent(Id, Email, StoreName, nowUtc));
    }

    /// <summary>Pending to Rejected (terminal). Notes are the reason shown to the applicant.</summary>
    public void Reject(int reviewerAppUserId, string? notes, DateTime nowUtc)
    {
        EnsurePending();
        ReviewedByAppUserId = DomainGuard.Positive(reviewerAppUserId, nameof(reviewerAppUserId));
        ReviewNotes = DomainGuard.Optional(notes, nameof(ReviewNotes), 1000);
        ReviewedAtUtc = nowUtc;
        Status = StoreApplicationStatus.Rejected;
        AddDomainEvent(new StoreApplicationRejectedEvent(Id, Email, StoreName, ReviewNotes, nowUtc));
    }

    private void EnsurePending()
    {
        if (Status != StoreApplicationStatus.Pending)
            throw new DomainValidationException($"Only a pending application can be reviewed (current status: {Status})");
    }
}
