using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;
using ThriftMedia.Domain.Events;
using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Tests.Domain;

public class StoreApplicationTests
{
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

    private static StoreApplication Submit(
        string email = "owner@example.com",
        string? country = null,
        string? websiteUrl = null,
        string licenseImageRef = "uploads/license.pdf",
        string storeName = "Second Chapter Books")
        => StoreApplication.Submit(
            storeName, "Pat Owner", email, "555-0100",
            "1 Main St", null, "Springfield", "IL", "62701", country,
            websiteUrl, "Used books and vinyl",
            "LIC-123", licenseImageRef, null, Now);

    private static StoreApplication WithId(StoreApplication a, int id = 3) =>
        StoreApplication.Rehydrate(id, a.Status, a.StoreName, a.OwnerFullName, a.Email, a.PhoneNumber,
            a.Address1, a.Address2, a.City, a.ProvinceState, a.PostalCode, a.Country,
            a.WebsiteUrl, a.Description, a.LicenseNumber, a.LicenseImageRef, a.StoreFrontImageRef,
            a.SubmittedAtUtc, a.ReviewedByAppUserId, a.ReviewedAtUtc, a.ReviewNotes, a.AppUserId);

    [Fact]
    public void Submit_CreatesPendingApplicationWithDefaults()
    {
        var app = Submit();

        Assert.Equal(StoreApplicationStatus.Pending, app.Status);
        Assert.Equal("United States", app.Country);
        Assert.Equal(string.Empty, app.Address2);
        Assert.Equal(Now, app.SubmittedAtUtc);
        Assert.Null(app.AppUserId);
    }

    [Fact]
    public void Submit_AcceptsUnitedStatesCaseInsensitively()
    {
        Assert.Equal("United States", Submit(country: "united states").Country);
    }

    [Fact]
    public void Submit_RejectsOtherCountries()
    {
        Assert.Throws<DomainValidationException>(() => Submit(country: "Canada"));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Submit_RejectsInvalidEmail(string email)
    {
        Assert.Throws<DomainValidationException>(() => Submit(email: email));
    }

    [Fact]
    public void Submit_RequiresLicenseImage()
    {
        Assert.Throws<DomainValidationException>(() => Submit(licenseImageRef: " "));
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("example.com")]
    public void Submit_RejectsNonHttpWebsite(string url)
    {
        Assert.Throws<DomainValidationException>(() => Submit(websiteUrl: url));
    }

    [Fact]
    public void Submit_AllowsHttpsWebsite()
    {
        Assert.Equal("https://example.com", Submit(websiteUrl: "https://example.com").WebsiteUrl);
    }

    [Fact]
    public void Submit_EnforcesMaxLength()
    {
        Assert.Throws<DomainValidationException>(() => Submit(storeName: new string('x', 101)));
    }

    [Fact]
    public void Approve_RecordsReviewAndRaisesEvent()
    {
        var app = WithId(Submit());
        var reviewedAt = Now.AddHours(1);

        app.Approve(reviewerAppUserId: 1, storeAdminAppUserId: 9, notes: " verified ", reviewedAt);

        Assert.Equal(StoreApplicationStatus.Approved, app.Status);
        Assert.Equal(9, app.AppUserId);
        Assert.Equal(1, app.ReviewedByAppUserId);
        Assert.Equal("verified", app.ReviewNotes);
        Assert.Equal(reviewedAt, app.ReviewedAtUtc);
        var evt = Assert.IsType<StoreApplicationApprovedEvent>(Assert.Single(app.DomainEvents));
        Assert.Equal("owner@example.com", evt.ApplicantEmail);
    }

    [Fact]
    public void Reject_RecordsReasonAndRaisesEvent()
    {
        var app = WithId(Submit());

        app.Reject(1, "License unreadable", Now);

        Assert.Equal(StoreApplicationStatus.Rejected, app.Status);
        Assert.Null(app.AppUserId);
        var evt = Assert.IsType<StoreApplicationRejectedEvent>(Assert.Single(app.DomainEvents));
        Assert.Equal("License unreadable", evt.Reason);
    }

    [Fact]
    public void Approve_RequiresValidIds()
    {
        var app = WithId(Submit());

        Assert.Throws<DomainValidationException>(() => app.Approve(0, 9, null, Now));
        Assert.Throws<DomainValidationException>(() => app.Approve(1, 0, null, Now));
        Assert.Equal(StoreApplicationStatus.Pending, app.Status);
    }

    [Fact]
    public void ReviewedApplication_CannotBeReviewedAgain()
    {
        var approved = WithId(Submit());
        approved.Approve(1, 9, null, Now);
        var rejected = WithId(Submit());
        rejected.Reject(1, null, Now);

        Assert.Throws<DomainValidationException>(() => approved.Reject(1, null, Now));
        Assert.Throws<DomainValidationException>(() => approved.Approve(1, 9, null, Now));
        Assert.Throws<DomainValidationException>(() => rejected.Approve(1, 9, null, Now));
        Assert.Throws<DomainValidationException>(() => rejected.Reject(1, null, Now));
    }
}
