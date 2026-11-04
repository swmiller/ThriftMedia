using Dapper;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;

namespace ThriftMedia.Infrastructure.Persistence;

internal sealed class StoreApplicationRepository(DbSession session) : IStoreApplicationRepository
{
    private const string SelectColumns = """
        "Id", "Status", "StoreName", "OwnerFullName", "Email", "PhoneNumber",
        "Address1", "Address2", "City", "ProvinceState", "PostalCode", "Country",
        "WebsiteUrl", "Description", "LicenseNumber", "LicenseImageRef", "StoreFrontImageRef",
        "SubmittedAtUtc", "ReviewedByAppUserId", "ReviewedAtUtc", "ReviewNotes", "AppUserId"
        """;

    public async Task<StoreApplication?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(
            $"""SELECT {SelectColumns} FROM retail."StoreApplication" WHERE "Id" = @id""",
            new { id }, session.Transaction, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    public async Task<IReadOnlyList<StoreApplication>> GetPendingAsync(CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<Row>(new CommandDefinition(
            $"""
            SELECT {SelectColumns} FROM retail."StoreApplication"
            WHERE "Status" = @status ORDER BY "SubmittedAtUtc", "Id"
            """,
            new { status = (int)StoreApplicationStatus.Pending }, session.Transaction, cancellationToken: cancellationToken));
        return rows.Select(r => r.ToEntity()).ToList();
    }

    public async Task<bool> HasPendingForEmailAsync(string email, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
            SELECT EXISTS (
                SELECT 1 FROM retail."StoreApplication"
                WHERE "Status" = @status AND LOWER("Email") = LOWER(@email))
            """,
            new { status = (int)StoreApplicationStatus.Pending, email = email.Trim() },
            session.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<int> AddAsync(StoreApplication application, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            INSERT INTO retail."StoreApplication"
                ("Status", "StoreName", "OwnerFullName", "Email", "PhoneNumber",
                 "Address1", "Address2", "City", "ProvinceState", "PostalCode", "Country",
                 "WebsiteUrl", "Description", "LicenseNumber", "LicenseImageRef", "StoreFrontImageRef",
                 "SubmittedAtUtc", "ReviewedByAppUserId", "ReviewedAtUtc", "ReviewNotes", "AppUserId")
            VALUES
                (@Status, @StoreName, @OwnerFullName, @Email, @PhoneNumber,
                 @Address1, @Address2, @City, @ProvinceState, @PostalCode, @Country,
                 @WebsiteUrl, @Description, @LicenseNumber, @LicenseImageRef, @StoreFrontImageRef,
                 @SubmittedAtUtc, @ReviewedByAppUserId, @ReviewedAtUtc, @ReviewNotes, @AppUserId)
            RETURNING "Id"
            """,
            ToParameters(application), session.Transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(StoreApplication application, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE retail."StoreApplication"
            SET "Status" = @Status, "ReviewedByAppUserId" = @ReviewedByAppUserId, "ReviewedAtUtc" = @ReviewedAtUtc,
                "ReviewNotes" = @ReviewNotes, "AppUserId" = @AppUserId
            WHERE "Id" = @Id
            """,
            ToParameters(application), session.Transaction, cancellationToken: cancellationToken));
    }

    private static object ToParameters(StoreApplication a) => new
    {
        a.Id,
        Status = (int)a.Status,
        a.StoreName,
        a.OwnerFullName,
        a.Email,
        a.PhoneNumber,
        a.Address1,
        a.Address2,
        a.City,
        a.ProvinceState,
        a.PostalCode,
        a.Country,
        a.WebsiteUrl,
        a.Description,
        a.LicenseNumber,
        a.LicenseImageRef,
        a.StoreFrontImageRef,
        SubmittedAtUtc = DbTime.ToDb(a.SubmittedAtUtc),
        a.ReviewedByAppUserId,
        ReviewedAtUtc = DbTime.ToDb(a.ReviewedAtUtc),
        a.ReviewNotes,
        a.AppUserId
    };

    private sealed class Row
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string StoreName { get; set; } = "";
        public string OwnerFullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address1 { get; set; } = "";
        public string Address2 { get; set; } = "";
        public string City { get; set; } = "";
        public string ProvinceState { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "";
        public string? WebsiteUrl { get; set; }
        public string Description { get; set; } = "";
        public string LicenseNumber { get; set; } = "";
        public string LicenseImageRef { get; set; } = "";
        public string? StoreFrontImageRef { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
        public int? ReviewedByAppUserId { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
        public string? ReviewNotes { get; set; }
        public int? AppUserId { get; set; }

        public StoreApplication ToEntity() => StoreApplication.Rehydrate(
            Id, (StoreApplicationStatus)Status, StoreName, OwnerFullName, Email, PhoneNumber,
            Address1, Address2, City, ProvinceState, PostalCode, Country,
            WebsiteUrl, Description, LicenseNumber, LicenseImageRef, StoreFrontImageRef,
            DbTime.FromDb(SubmittedAtUtc), ReviewedByAppUserId, DbTime.FromDb(ReviewedAtUtc), ReviewNotes, AppUserId);
    }
}
