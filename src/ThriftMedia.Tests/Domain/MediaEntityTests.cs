using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Domain.ValueObjects;

namespace ThriftMedia.Tests.Domain;

public class MediaEntityTests
{
    private const string ValidOcrJson = """{"title": "Test Book", "author": "Test Author"}""";
    private static readonly Uri ValidImageUri = new("https://example.com/image.jpg");
    private static readonly MediaType ValidMediaType = MediaType.Book;
    private const int ValidStoreId = 1;
    private const string CreatedBy = "test-user";
    private static readonly DateTime TestTime = new(2025, 10, 22, 12, 0, 0, DateTimeKind.Utc);

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ReturnsMediaEntity()
    {
        // Act
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);

        // Assert
        Assert.NotNull(media);
        Assert.Equal(0, media.Id); // Not yet persisted
        Assert.Equal(ValidStoreId, media.StoreId);
        Assert.Equal(ValidMediaType, media.Type);
        Assert.Equal(ValidImageUri, media.ImageUri);
        Assert.Equal(ValidOcrJson, media.OcrPayloadJson);
        Assert.False(media.IsExplicitContent);
        Assert.Equal(CreatedBy, media.Audit.CreatedBy);
        Assert.Equal(TestTime, media.Audit.CreatedAtUtc);
        Assert.Null(media.Audit.UpdatedBy);
        Assert.Null(media.Audit.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidStoreId_ThrowsDomainValidationException(int invalidStoreId)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Media.Create(invalidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime));

        Assert.Contains("StoreId", exception.Message);
    }

    [Fact]
    public void Create_WithNullMediaType_ThrowsDomainValidationException()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Media.Create(ValidStoreId, null!, ValidImageUri, ValidOcrJson, CreatedBy, TestTime));

        Assert.Contains("MediaType is required", exception.Message);
    }

    [Fact]
    public void Create_WithNullImageUri_ThrowsDomainValidationException()
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Media.Create(ValidStoreId, ValidMediaType, null!, ValidOcrJson, CreatedBy, TestTime));

        Assert.Contains("ImageUri is required", exception.Message);
    }

    [Fact]
    public void Create_WithRelativeImageUri_ThrowsDomainValidationException()
    {
        // Arrange
        var relativeUri = new Uri("/relative/path", UriKind.Relative);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Media.Create(ValidStoreId, ValidMediaType, relativeUri, ValidOcrJson, CreatedBy, TestTime));

        Assert.Contains("absolute", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOcrJson_ThrowsDomainValidationException(string? ocrJson)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ocrJson, CreatedBy, TestTime));

        Assert.Contains("OcrPayloadJson is required", exception.Message);
    }

    [Fact]
    public void Create_WithInvalidJson_ThrowsDomainValidationException()
    {
        // Arrange
        var invalidJson = "{this is not valid json}";

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, invalidJson, CreatedBy, TestTime));

        Assert.Contains("not valid JSON", exception.Message);
    }

    #endregion

    #region UpdateOcr Tests

    [Fact]
    public void UpdateOcr_WithValidJson_UpdatesOcrAndAudit()
    {
        // Arrange
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);
        var newOcrJson = """{"title": "Updated Book", "author": "Updated Author"}""";
        var updatedBy = "updater-user";
        var updateTime = TestTime.AddHours(1);

        // Act
        media.UpdateOcr(newOcrJson, updatedBy, updateTime);

        // Assert
        Assert.Equal(newOcrJson, media.OcrPayloadJson);
        Assert.Equal(updatedBy, media.Audit.UpdatedBy);
        Assert.Equal(updateTime, media.Audit.UpdatedAtUtc);
        Assert.Equal(CreatedBy, media.Audit.CreatedBy); // Original creator unchanged
        Assert.Equal(TestTime, media.Audit.CreatedAtUtc); // Original time unchanged
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateOcr_WithNullOrEmptyJson_ThrowsDomainValidationException(string? ocrJson)
    {
        // Arrange
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            media.UpdateOcr(ocrJson, "updater", TestTime.AddHours(1)));

        Assert.Contains("OcrPayloadJson is required", exception.Message);
    }

    [Fact]
    public void UpdateOcr_WithInvalidJson_ThrowsDomainValidationException()
    {
        // Arrange
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);
        var invalidJson = "not valid json at all";

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() =>
            media.UpdateOcr(invalidJson, "updater", TestTime.AddHours(1)));

        Assert.Contains("not valid JSON", exception.Message);
    }

    #endregion

    #region FlagAsExplicitContent Tests

    [Fact]
    public void FlagAsExplicitContent_SetsFlag_UpdatesAudit()
    {
        // Arrange
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);
        var updatedBy = "moderator";
        var updateTime = TestTime.AddHours(1);

        // Act
        media.FlagAsExplicitContent(updatedBy, updateTime);

        // Assert
        Assert.True(media.IsExplicitContent);
        Assert.Equal(updatedBy, media.Audit.UpdatedBy);
        Assert.Equal(updateTime, media.Audit.UpdatedAtUtc);
    }

    [Fact]
    public void FlagAsExplicitContent_IsIdempotent()
    {
        // Arrange
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);
        var firstUpdater = "moderator1";
        var firstUpdateTime = TestTime.AddHours(1);
        media.FlagAsExplicitContent(firstUpdater, firstUpdateTime);

        // Act - flag again
        var secondUpdater = "moderator2";
        var secondUpdateTime = TestTime.AddHours(2);
        media.FlagAsExplicitContent(secondUpdater, secondUpdateTime);

        // Assert - first update is preserved
        Assert.True(media.IsExplicitContent);
        Assert.Equal(firstUpdater, media.Audit.UpdatedBy);
        Assert.Equal(firstUpdateTime, media.Audit.UpdatedAtUtc);
    }

    #endregion

    #region ClassifyContent Tests

    [Fact]
    public void ClassifyContent_ReturnsStubValue()
    {
        // Arrange
        var media = Media.Create(ValidStoreId, ValidMediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);

        // Act
        var result = media.ClassifyContent();

        // Assert - stub currently returns false
        Assert.False(result);
    }

    #endregion

    #region Multiple MediaType Tests

    [Theory]
    [InlineData("book")]
    [InlineData("video")]
    [InlineData("cdrom")]
    [InlineData("vinyl-record")]
    [InlineData("eight-track")]
    [InlineData("cassette")]
    [InlineData("dvd")]
    [InlineData("blu-ray")]
    [InlineData("magazine")]
    [InlineData("comic")]
    [InlineData("other")]
    [InlineData("unknown")]
    public void Create_WithAllValidMediaTypes_Succeeds(string mediaTypeValue)
    {
        // Arrange
        var mediaType = MediaType.From(mediaTypeValue);

        // Act
        var media = Media.Create(ValidStoreId, mediaType, ValidImageUri, ValidOcrJson, CreatedBy, TestTime);

        // Assert
        Assert.NotNull(media);
        Assert.Equal(mediaType, media.Type);
    }

    #endregion
}
