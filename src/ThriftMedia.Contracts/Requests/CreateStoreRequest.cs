using System;
using System.Collections.Generic;
using System.Text;

namespace ThriftMedia.Contracts.Requests;

public record CreateStoreRequest(
    string StoreName,
    string? PhoneNumber,
    string? WebsiteUrl,
    string? OwnerFirstName,
    string? OwnerLastName,
    string? OwnerPhoneNumber,
    string? OwnerEmail,
    string LicenseNumber,
    string LicenseType,
    string IssueingAuthority,
    DateTime IssueDate,
    DateTime? ExpirationDate,
    string LicenseStatus,
    string Address1,
    string Address2,
    string City,
    string PostalCode,
    string? Country,
    string? ProvinceState,
    int AppUserId
);