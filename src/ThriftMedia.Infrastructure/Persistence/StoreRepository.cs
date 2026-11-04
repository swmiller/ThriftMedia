using Dapper;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Infrastructure.Persistence;

internal sealed class StoreRepository(DbSession session) : IStoreRepository
{
    public async Task<int> AddAsync(Store store, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            INSERT INTO retail."Store"
                ("StoreName", "PhoneNumber", "WebsiteUrl", "IsActive", "IsSuspended",
                 "OwnerFirstName", "OwnerLastName", "OwnerPhoneNumber", "OwnerEmail",
                 "LicenseNumber", "LicenseType", "IssueingAuthority", "IssueDate", "ExpirationDate", "LicenseStatus",
                 "Address1", "Address2", "City", "ProvinceState", "PostalCode", "Country",
                 "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "AppUserId")
            VALUES
                (@StoreName, @PhoneNumber, @WebsiteUrl, @IsActive, @IsSuspended,
                 @OwnerFirstName, @OwnerLastName, @OwnerPhoneNumber, @OwnerEmail,
                 @LicenseNumber, @LicenseType, @IssueingAuthority, @IssueDate, @ExpirationDate, @LicenseStatus,
                 @Address1, @Address2, @City, @ProvinceState, @PostalCode, @Country,
                 @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt, @AppUserId)
            RETURNING "Id"
            """,
            new
            {
                store.StoreName,
                store.PhoneNumber,
                store.WebsiteUrl,
                store.IsActive,
                store.IsSuspended,
                store.OwnerFirstName,
                store.OwnerLastName,
                store.OwnerPhoneNumber,
                store.OwnerEmail,
                store.LicenseNumber,
                store.LicenseType,
                store.IssueingAuthority,
                IssueDate = DbTime.ToDb(store.IssueDate),
                ExpirationDate = DbTime.ToDb(store.ExpirationDate),
                store.LicenseStatus,
                Address1 = store.Address.Line1,
                Address2 = store.Address.Line2 ?? string.Empty,
                store.Address.City,
                store.Address.ProvinceState,
                store.Address.PostalCode,
                store.Address.Country,
                store.CreatedBy,
                CreatedAt = DbTime.ToDb(store.CreatedAt),
                store.UpdatedBy,
                UpdatedAt = DbTime.ToDb(store.UpdatedAt),
                store.AppUserId
            },
            session.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<int?> GetIdByAppUserIdAsync(int appUserId, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int?>(new CommandDefinition(
            """SELECT "Id" FROM retail."Store" WHERE "AppUserId" = @appUserId""",
            new { appUserId }, session.Transaction, cancellationToken: cancellationToken));
    }
}
