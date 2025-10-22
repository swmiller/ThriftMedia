using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Tests.Domain;

public class StoreEntityTests
{
    private const string ValidName = "Test Store";
    private const string CreatedBy = "test-user";
    private static readonly DateTime TestTime = new(2025, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Address ValidAddress = Address.Create(
        "123 Main St",
        "Portland",
        "OR",
        "97201",
        "USA"
    );

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ReturnsStoreEntity()
    {
        // Act
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Assert
        Assert.NotNull(store);
        Assert.Equal(0, store.Id); // Not yet persisted
        Assert.Equal(ValidName, store.Name);
        Assert.Equal(ValidAddress, store.Address);
        Assert.False(store.IsAuthorized); // No license image yet
        Assert.False(store.ExplicitContentFlagged);
        Assert.False(store.IsDisabled);
        Assert.Equal(CreatedBy, store.Audit.CreatedBy);
        Assert.Equal(TestTime, store.Audit.CreatedAtUtc);
        Assert.Null(store.Audit.UpdatedBy);
        Assert.Null(store.Audit.UpdatedAtUtc);
        Assert.Null(store.BusinessLicenseImageUri);
    }

    [Fact]
    public void Create_WithNullAddress_ThrowsDomainValidationException()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Store.Create(ValidName, null!, CreatedBy, TestTime));

        Assert.Contains("Address is required", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyName_ThrowsDomainValidationException(string? name)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Store.Create(name!, ValidAddress, CreatedBy, TestTime));

        Assert.Contains("Store name required", exception.Message);
    }

    [Fact]
    public void Create_WithNameTooLong_ThrowsDomainValidationException()
    {
        // Arrange
        var longName = new string('X', 201); // Max is 200

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Store.Create(longName, ValidAddress, CreatedBy, TestTime));

        Assert.Contains("Store name too long", exception.Message);
    }

    [Fact]
    public void Create_WithNameExactly200Chars_Succeeds()
    {
        // Arrange
        var maxLengthName = new string('X', 200);

        // Act
        var store = Store.Create(maxLengthName, ValidAddress, CreatedBy, TestTime);

        // Assert
        Assert.NotNull(store);
        Assert.Equal(maxLengthName, store.Name);
    }

    [Fact]
    public void Create_TrimsWhitespaceFromName()
    {
        // Arrange
        var nameWithWhitespace = "  Test Store  ";

        // Act
        var store = Store.Create(nameWithWhitespace, ValidAddress, CreatedBy, TestTime);

        // Assert
        Assert.Equal("Test Store", store.Name);
    }

    #endregion

    #region Rename Tests

    [Fact]
    public void Rename_WithValidName_UpdatesNameAndAudit()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var newName = "Updated Store Name";
        var updatedBy = "updater-user";
        var updateTime = TestTime.AddHours(1);

        // Act
        store.Rename(newName, updatedBy, updateTime);

        // Assert
        Assert.Equal(newName, store.Name);
        Assert.Equal(updatedBy, store.Audit.UpdatedBy);
        Assert.Equal(updateTime, store.Audit.UpdatedAtUtc);
        Assert.Equal(CreatedBy, store.Audit.CreatedBy); // Original creator unchanged
        Assert.Equal(TestTime, store.Audit.CreatedAtUtc); // Original time unchanged
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithNullOrEmptyName_ThrowsDomainValidationException(string? newName)
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            store.Rename(newName!, "updater", TestTime.AddHours(1)));

        Assert.Contains("Name required", exception.Message);
    }

    [Fact]
    public void Rename_WithNameTooLong_ThrowsDomainValidationException()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var longName = new string('X', 201);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            store.Rename(longName, "updater", TestTime.AddHours(1)));

        Assert.Contains("Name too long", exception.Message);
    }

    [Fact]
    public void Rename_TrimsWhitespace()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var nameWithWhitespace = "  Updated Name  ";

        // Act
        store.Rename(nameWithWhitespace, "updater", TestTime.AddHours(1));

        // Assert
        Assert.Equal("Updated Name", store.Name);
    }

    #endregion

    #region ChangeAddress Tests

    [Fact]
    public void ChangeAddress_WithValidAddress_UpdatesAddressAndAudit()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var newAddress = Address.Create("456 Oak Ave", "Seattle", "WA", "98101", "USA");
        var updatedBy = "updater-user";
        var updateTime = TestTime.AddHours(1);

        // Act
        store.ChangeAddress(newAddress, updatedBy, updateTime);

        // Assert
        Assert.Equal(newAddress, store.Address);
        Assert.Equal(updatedBy, store.Audit.UpdatedBy);
        Assert.Equal(updateTime, store.Audit.UpdatedAtUtc);
    }

    [Fact]
    public void ChangeAddress_WithNullAddress_ThrowsDomainValidationException()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            store.ChangeAddress(null!, "updater", TestTime.AddHours(1)));

        Assert.Contains("Address is required", exception.Message);
    }

    #endregion

    #region SetBusinessLicenseImage Tests

    [Fact]
    public void SetBusinessLicenseImage_WithValidUri_SetsUriAndUpdatesAudit()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var licenseUri = new Uri("https://example.com/license.jpg");
        var updatedBy = "admin-user";
        var updateTime = TestTime.AddHours(1);

        // Act
        store.SetBusinessLicenseImage(licenseUri, updatedBy, updateTime);

        // Assert
        Assert.Equal(licenseUri, store.BusinessLicenseImageUri);
        Assert.True(store.IsAuthorized); // Now authorized
        Assert.Equal(updatedBy, store.Audit.UpdatedBy);
        Assert.Equal(updateTime, store.Audit.UpdatedAtUtc);
    }

    [Fact]
    public void SetBusinessLicenseImage_WithNullUri_ThrowsDomainValidationException()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            store.SetBusinessLicenseImage(null!, "updater", TestTime.AddHours(1)));

        Assert.Contains("absolute URI", exception.Message);
    }

    [Fact]
    public void SetBusinessLicenseImage_WithRelativeUri_ThrowsDomainValidationException()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var relativeUri = new Uri("/relative/path", UriKind.Relative);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            store.SetBusinessLicenseImage(relativeUri, "updater", TestTime.AddHours(1)));

        Assert.Contains("absolute URI", exception.Message);
    }

    [Fact]
    public void SetBusinessLicenseImage_CanReplaceExistingLicense()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var firstUri = new Uri("https://example.com/license1.jpg");
        var secondUri = new Uri("https://example.com/license2.jpg");

        store.SetBusinessLicenseImage(firstUri, "admin", TestTime.AddHours(1));

        // Act
        store.SetBusinessLicenseImage(secondUri, "admin", TestTime.AddHours(2));

        // Assert
        Assert.Equal(secondUri, store.BusinessLicenseImageUri);
        Assert.True(store.IsAuthorized);
    }

    #endregion

    #region IsAuthorized Tests

    [Fact]
    public void IsAuthorized_WithoutLicenseImage_ReturnsFalse()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Assert
        Assert.False(store.IsAuthorized);
    }

    [Fact]
    public void IsAuthorized_WithLicenseImage_ReturnsTrue()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var licenseUri = new Uri("https://example.com/license.jpg");
        store.SetBusinessLicenseImage(licenseUri, "admin", TestTime.AddHours(1));

        // Assert
        Assert.True(store.IsAuthorized);
    }

    #endregion

    #region FlagExplicitContent Tests

    [Fact]
    public void FlagExplicitContent_SetsFlag_UpdatesAudit()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var updatedBy = "moderator";
        var updateTime = TestTime.AddHours(1);

        // Act
        store.FlagExplicitContent(updatedBy, updateTime);

        // Assert
        Assert.True(store.ExplicitContentFlagged);
        Assert.True(store.IsDisabled); // Disabled due to explicit content
        Assert.Equal(updatedBy, store.Audit.UpdatedBy);
        Assert.Equal(updateTime, store.Audit.UpdatedAtUtc);
    }

    [Fact]
    public void FlagExplicitContent_IsIdempotent()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        var firstUpdater = "moderator1";
        var firstUpdateTime = TestTime.AddHours(1);
        store.FlagExplicitContent(firstUpdater, firstUpdateTime);

        // Act - flag again
        var secondUpdater = "moderator2";
        var secondUpdateTime = TestTime.AddHours(2);
        store.FlagExplicitContent(secondUpdater, secondUpdateTime);

        // Assert - first update is preserved
        Assert.True(store.ExplicitContentFlagged);
        Assert.True(store.IsDisabled);
        Assert.Equal(firstUpdater, store.Audit.UpdatedBy);
        Assert.Equal(firstUpdateTime, store.Audit.UpdatedAtUtc);
    }

    #endregion

    #region IsDisabled Tests

    [Fact]
    public void IsDisabled_WithoutExplicitContent_ReturnsFalse()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Assert
        Assert.False(store.IsDisabled);
    }

    [Fact]
    public void IsDisabled_WithExplicitContent_ReturnsTrue()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);
        store.FlagExplicitContent("moderator", TestTime.AddHours(1));

        // Assert
        Assert.True(store.IsDisabled);
    }

    #endregion

    #region VerifyAddressAgainstExternalService Tests

    [Fact]
    public void VerifyAddressAgainstExternalService_ReturnsStubValue()
    {
        // Arrange
        var store = Store.Create(ValidName, ValidAddress, CreatedBy, TestTime);

        // Act
        var result = store.VerifyAddressAgainstExternalService();

        // Assert - stub currently returns true
        Assert.True(result);
    }

    #endregion

    #region Integration Tests - Multiple Operations

    [Fact]
    public void CompleteStoreLifecycle_AllOperationsWork()
    {
        // Create
        var store = Store.Create("Original Name", ValidAddress, "creator", TestTime);
        Assert.Equal("Original Name", store.Name);
        Assert.False(store.IsAuthorized);
        Assert.False(store.IsDisabled);

        // Rename
        store.Rename("New Name", "updater1", TestTime.AddHours(1));
        Assert.Equal("New Name", store.Name);

        // Change Address
        var newAddress = Address.Create("789 Elm St", "Denver", "CO", "80201", "USA");
        store.ChangeAddress(newAddress, "updater2", TestTime.AddHours(2));
        Assert.Equal(newAddress, store.Address);

        // Add License (authorize)
        var licenseUri = new Uri("https://example.com/license.jpg");
        store.SetBusinessLicenseImage(licenseUri, "admin", TestTime.AddHours(3));
        Assert.True(store.IsAuthorized);

        // Flag explicit content (disable)
        store.FlagExplicitContent("moderator", TestTime.AddHours(4));
        Assert.True(store.IsDisabled);

        // Verify audit trail
        Assert.Equal("creator", store.Audit.CreatedBy);
        Assert.Equal(TestTime, store.Audit.CreatedAtUtc);
        Assert.Equal("moderator", store.Audit.UpdatedBy);
        Assert.Equal(TestTime.AddHours(4), store.Audit.UpdatedAtUtc);
    }

    #endregion
}
