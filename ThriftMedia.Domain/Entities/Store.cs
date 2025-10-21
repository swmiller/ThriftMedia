using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Domain.Entities;

public sealed class Store
{
    public int Id { get; private set; } // 0 until persistence assigns identity
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public AuditMetadata Audit { get; private set; }
    /// <summary>
    /// Absolute URI to an image (scan/photo) of the store's business license. Required for authorization.
    /// </summary>
    public Uri? BusinessLicenseImageUri { get; private set; }
    /// <summary>
    /// A store is authorized for the platform only after a business license image has been provided (and later, validated).
    /// </summary>
    public bool IsAuthorized => BusinessLicenseImageUri is not null;
    /// <summary>
    /// True once any media associated with this store has been flagged for explicit / pornographic content.
    /// Irreversible by default (can add a clearing workflow later if moderation overturns a decision).
    /// </summary>
    public bool ExplicitContentFlagged { get; private set; }
    /// <summary>
    /// Store is considered disabled if explicit content has been detected. Additional disable criteria can be added later.
    /// </summary>
    public bool IsDisabled => ExplicitContentFlagged;

    private Store(string name, Address address, AuditMetadata audit)
    {
        Name = name;
        Address = address;
        Audit = audit;
    }

    public static Store Create(string name, Address address, string createdBy, DateTime nowUtc)
    {
        if (address is null) throw new DomainValidationException("Address is required");
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException("Store name required");
        if (name.Length > 200) throw new DomainValidationException("Store name too long");
        return new Store(name, address, AuditMetadata.Create(createdBy, nowUtc));
    }

    public void Rename(string newName, string updatedBy, DateTime nowUtc)
    {
        newName = (newName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(newName)) throw new DomainValidationException("Name required");
        if (newName.Length > 200) throw new DomainValidationException("Name too long");
        Name = newName;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    public void ChangeAddress(Address newAddress, string updatedBy, DateTime nowUtc)
    {
        if (newAddress is null) throw new DomainValidationException("Address is required");
        Address = newAddress;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    /// <summary>
    /// Sets or replaces the business license image URI. This enables authorization if previously unset.
    /// </summary>
    public void SetBusinessLicenseImage(Uri licenseImageUri, string updatedBy, DateTime nowUtc)
    {
        if (licenseImageUri is null || !licenseImageUri.IsAbsoluteUri)
            throw new DomainValidationException("Business license image URI must be an absolute URI");
        BusinessLicenseImageUri = licenseImageUri;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }

    /// <summary>
    /// Stub for future external address verification integration. Currently returns true.
    /// </summary>
    public bool VerifyAddressAgainstExternalService()
    {
        // TODO: Integrate address verification provider (e.g., USPS, Loqate, Google Places, etc.)
        return true;
    }

    /// <summary>
    /// Flags the store due to explicit / pornographic content in one or more media assets.
    /// Idempotent: repeated calls have no further effect.
    /// </summary>
    public void FlagExplicitContent(string updatedBy, DateTime nowUtc)
    {
        if (ExplicitContentFlagged) return; // idempotent
        ExplicitContentFlagged = true;
        Audit = Audit.WithUpdated(updatedBy, nowUtc);
    }
}
