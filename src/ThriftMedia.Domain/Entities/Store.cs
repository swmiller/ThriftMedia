using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Domain.Entities;

public sealed class Store
{
    public int Id { get; private set; } // 0 until persistence assigns identity

    public string StoreName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSuspended { get; private set; }
    public string? OwnerFirstName { get; private set; }
    public string? OwnerLastName { get; private set; }
    public string? OwnerPhoneNumber { get; private set; }
    public string? OwnerEmail { get; private set; }
    public string LicenseNumber { get; private set; }
    public string LicenseType { get; private set; }
    public string IssueingAuthority { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string LicenseStatus { get; private set; }
    public string Address1 { get; private set; }
    public string Address2 { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int AppUserId { get; private set; }
    public string? ProvinceState { get; private set; }

    private Store(
        string storeName,
        string licenseNumber,
        string licenseType,
        string issueingAuthority,
        DateTime issueDate,
        DateTime? expirationDate,
        string licenseStatus,
        string address1,
        string address2,
        string city,
        string postalCode,
        string? country,
        int appUserId,
        string? provinceState,
        string createdBy,
        DateTime createdAt)
    {
        StoreName = ValidateRequired(storeName, nameof(StoreName), 100);
        LicenseNumber = ValidateRequired(licenseNumber, nameof(LicenseNumber), 100);
        LicenseType = ValidateRequired(licenseType, nameof(LicenseType), 50);
        IssueingAuthority = ValidateRequired(issueingAuthority, nameof(IssueingAuthority), 100);
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        LicenseStatus = ValidateRequired(licenseStatus, nameof(LicenseStatus), 20);
        Address1 = ValidateRequired(address1, nameof(Address1), 150);
        Address2 = ValidateRequired(address2, nameof(Address2), 150);
        City = ValidateRequired(city, nameof(City), 100);
        PostalCode = ValidateRequired(postalCode, nameof(PostalCode), 20);
        Country = ValidateOptional(country, nameof(Country), 100);
        AppUserId = ValidatePositive(appUserId, nameof(AppUserId));
        ProvinceState = ValidateOptional(provinceState, nameof(ProvinceState), 50);
        CreatedBy = ValidateRequired(createdBy, nameof(CreatedBy), 100);
        CreatedAt = createdAt;

        IsActive = true;
        IsSuspended = false;

        ValidateDateRange(IssueDate, ExpirationDate);
    }

    public static Store Create(
        string storeName,
        string licenseNumber,
        string licenseType,
        string issueingAuthority,
        DateTime issueDate,
        DateTime? expirationDate,
        string licenseStatus,
        string address1,
        string address2,
        string city,
        string postalCode,
        string? country,
        int appUserId,
        string? provinceState,
        string createdBy,
        DateTime createdAt)
        => new(
            storeName,
            licenseNumber,
            licenseType,
            issueingAuthority,
            issueDate,
            expirationDate,
            licenseStatus,
            address1,
            address2,
            city,
            postalCode,
            country,
            appUserId,
            provinceState,
            createdBy,
            createdAt);

    public void Rename(string newStoreName, string updatedBy, DateTime nowUtc)
    {
        StoreName = ValidateRequired(newStoreName, nameof(StoreName), 100);
        MarkUpdated(updatedBy, nowUtc);
    }

    public void ChangeAddress(
        string address1,
        string address2,
        string city,
        string postalCode,
        string? provinceState,
        string? country,
        string updatedBy,
        DateTime nowUtc)
    {
        Address1 = ValidateRequired(address1, nameof(Address1), 150);
        Address2 = ValidateRequired(address2, nameof(Address2), 150);
        City = ValidateRequired(city, nameof(City), 100);
        PostalCode = ValidateRequired(postalCode, nameof(PostalCode), 20);
        ProvinceState = ValidateOptional(provinceState, nameof(ProvinceState), 50);
        Country = ValidateOptional(country, nameof(Country), 100);
        MarkUpdated(updatedBy, nowUtc);
    }

    public void UpdateContactDetails(string? phoneNumber, string? websiteUrl, string updatedBy, DateTime nowUtc)
    {
        PhoneNumber = ValidateOptional(phoneNumber, nameof(PhoneNumber), 50);
        WebsiteUrl = ValidateOptional(websiteUrl, nameof(WebsiteUrl), 255);

        if (WebsiteUrl is not null && !Uri.TryCreate(WebsiteUrl, UriKind.Absolute, out _))
            throw new DomainValidationException("WebsiteUrl must be a valid absolute URI");

        MarkUpdated(updatedBy, nowUtc);
    }

    public void UpdateOwnerDetails(
        string? ownerFirstName,
        string? ownerLastName,
        string? ownerPhoneNumber,
        string? ownerEmail,
        string updatedBy,
        DateTime nowUtc)
    {
        OwnerFirstName = ValidateOptional(ownerFirstName, nameof(OwnerFirstName), 50);
        OwnerLastName = ValidateOptional(ownerLastName, nameof(OwnerLastName), 50);
        OwnerPhoneNumber = ValidateOptional(ownerPhoneNumber, nameof(OwnerPhoneNumber), 50);
        OwnerEmail = ValidateOptional(ownerEmail, nameof(OwnerEmail), 255);

        if (OwnerEmail is not null && !IsPlausibleEmail(OwnerEmail))
            throw new DomainValidationException("OwnerEmail must be a valid email address");

        MarkUpdated(updatedBy, nowUtc);
    }

    public void UpdateLicense(
        string licenseNumber,
        string licenseType,
        string issueingAuthority,
        DateTime issueDate,
        DateTime? expirationDate,
        string licenseStatus,
        string updatedBy,
        DateTime nowUtc)
    {
        LicenseNumber = ValidateRequired(licenseNumber, nameof(LicenseNumber), 100);
        LicenseType = ValidateRequired(licenseType, nameof(LicenseType), 50);
        IssueingAuthority = ValidateRequired(issueingAuthority, nameof(IssueingAuthority), 100);
        LicenseStatus = ValidateRequired(licenseStatus, nameof(LicenseStatus), 20);
        IssueDate = issueDate;
        ExpirationDate = expirationDate;

        ValidateDateRange(IssueDate, ExpirationDate);
        MarkUpdated(updatedBy, nowUtc);
    }

    public void Suspend(string updatedBy, DateTime nowUtc)
    {
        IsSuspended = true;
        IsActive = false;
        MarkUpdated(updatedBy, nowUtc);
    }

    public void Reinstate(string updatedBy, DateTime nowUtc)
    {
        IsSuspended = false;
        IsActive = true;
        MarkUpdated(updatedBy, nowUtc);
    }

    public void Deactivate(string updatedBy, DateTime nowUtc)
    {
        IsActive = false;
        MarkUpdated(updatedBy, nowUtc);
    }

    public void Activate(string updatedBy, DateTime nowUtc)
    {
        if (IsSuspended)
            throw new DomainValidationException("Cannot activate a suspended store");

        IsActive = true;
        MarkUpdated(updatedBy, nowUtc);
    }

    private void MarkUpdated(string updatedBy, DateTime nowUtc)
    {
        UpdatedBy = ValidateRequired(updatedBy, nameof(UpdatedBy), 100);
        UpdatedAt = nowUtc;
    }

    private static string ValidateRequired(string? value, string fieldName, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw new DomainValidationException($"{fieldName} is required");
        if (trimmed.Length > maxLength) throw new DomainValidationException($"{fieldName} max length is {maxLength}");
        return trimmed;
    }

    private static string? ValidateOptional(string? value, string fieldName, int maxLength)
    {
        if (value is null) return null;

        var trimmed = value.Trim();
        if (trimmed.Length == 0) return null;
        if (trimmed.Length > maxLength) throw new DomainValidationException($"{fieldName} max length is {maxLength}");
        return trimmed;
    }

    private static int ValidatePositive(int value, string fieldName)
    {
        if (value <= 0) throw new DomainValidationException($"{fieldName} must be greater than zero");
        return value;
    }

    private static void ValidateDateRange(DateTime issueDate, DateTime? expirationDate)
    {
        if (expirationDate.HasValue && expirationDate.Value < issueDate)
            throw new DomainValidationException("ExpirationDate cannot be earlier than IssueDate");
    }

    private static bool IsPlausibleEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        return atIndex > 0 && atIndex < email.Length - 1;
    }
}
