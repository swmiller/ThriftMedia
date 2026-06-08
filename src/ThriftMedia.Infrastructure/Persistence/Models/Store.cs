using System;
using System.Collections.Generic;

namespace ThriftMedia.Infrastructure.Persistence.Models;

public partial class Store
{
    public int Id { get; set; }

    public string StoreName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; }

    public bool IsSuspended { get; set; }

    public string? OwnerFirstName { get; set; }

    public string? OwnerLastName { get; set; }

    public string? OwerPhoneNumber { get; set; }

    public string? OwerEmail { get; set; }

    public string LicenseNumber { get; set; } = null!;

    public string LicenseType { get; set; } = null!;

    public string IssueingAuthority { get; set; } = null!;

    public DateTime IssueDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string LicenseStatus { get; set; } = null!;

    public string Address1 { get; set; } = null!;

    public string Address2 { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Postcode { get; set; } = null!;

    public string? Country { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int AppUserId { get; set; }

    public virtual AppUser AppUser { get; set; } = null!;

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();
}
